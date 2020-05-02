using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Common;
using Code.Configuration;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Gameplay;
using Sample.Shared.Data.Storage;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Client.Providers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests.Auth;
using Shaman.Messages.General.DTO.Responses.Auth;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.RoomFlow;
using UnityEngine;

namespace Code.Network
{
    public interface IUnityClientPeer
    {
        Action OnConnected { get; set; }
        Action OnDisconnected { get; set; }
        
        void Initialize(float receiveRate = 50, int syncersQueueProcessIntervalMs = 33);
        Task Connect();
        void Disconnect();

        Task<JoinInfo> JoinGame(Dictionary<byte, object> matchMakingProperties,
            Dictionary<byte, object> joinGameProperties);

        Guid RegisterEventHandler<T>(Action<T> handler, bool callOnce = false)
            where T : MessageBase, new();
        
        void UnregisterOperationHandler(Guid handlerId);

        Task<T> SendRequest<T>(HttpRequestBase request, string url = null)
            where T : HttpResponseBase, new();

        Task<T> SendRequest<T>(RequestBase request) where T : ResponseBase, new();

        void SendEvent(EventBase eve);
    }
    
    public class UnityClientPeer : MonoBehaviour, IUnityClientPeer, IShamanClientPeerListener
    {
        //for DI
        [Inject]
        public IShamanLogger Logger { get; set; }

        [Inject] 
        public INetworkConfiguration NetworkConfiguration;

        [Inject]
        public IStorageContainer StorageContainer { get; set; }
        [Inject]
        public IClientServerInfoProvider ServerProvider { get; set; }
        [Inject]
        public IShamanClientPeer ClientPeer { get; set; }
        [Inject]
        public ITaskScheduler taskScheduler;

        [HideInInspector] public Action<Player> OnPlayerUpdatedEvent;
        [HideInInspector] public Action OnConnected { get; set; }
        [HideInInspector] public Action OnDisconnected { get; set; }
        [HideInInspector] public Action OnConnectionFailed;
        [HideInInspector] public Action OnGameNeedToBeUpdated;
        [HideInInspector] public Action<StorageContainerStatus> OnStorageStatusChanged;

        public DateTime? LastPlayerUpdateTime = null;
        
        public DataStorage Storage
        {
            get
            {
                if (!StorageContainer.IsReadyForRequests())
                {
                    Logger.Error($"Storage is not ready for requests");
                    return null;
                }

                return StorageContainer.GetStorage();
            }
        }

        //public int Rtt => ClientPeer.Rtt;
    
        public Player Player => Storage?.Player;

        public bool IsConnectedToRoom => (ConnectionStatus != null && ConnectionStatus.Status == ShamanClientStatus.InRoom);

        //private for using
        private float _receiveRatePerSec;
        private int _syncersProcessQueuesIntervalMs;
        private object _queueSync = new object();
        private Queue<EventBase> _sendQueue = new Queue<EventBase>();    
        private float _lastSentOn = 0, _lastReceivedOn = 0, _lastQueuesProcessedOn = 0;

        private bool _isSelfDisconnect;
    
        //important flow variables
        private ShamanConnectionStatus ConnectionStatus;
        private Route _route;
        public Guid SessionId;
        private Guid _authToken;
        private string _guestId
        {
            get
            {
                if (!PlayerPrefs.HasKey ("GuestId"))
                    return Guid.NewGuid().ToString();
                else
                    return PlayerPrefs.GetString ("GuestId");
            }
            set => PlayerPrefs.SetString("GuestId", value);
        }    
    
        public void Initialize(float receiveRate = 50, int syncersQueueProcessIntervalMs = 33)
        {
            _syncersProcessQueuesIntervalMs = syncersQueueProcessIntervalMs;
            _receiveRatePerSec = receiveRate;
            _lastReceivedOn = Time.time;
            _lastQueuesProcessedOn = Time.time;
            ClientPeer.OnDisconnected += (reason) =>
            {
                if (!_isSelfDisconnect)
                    this.Logger.Error($"Disconnected on {ClientPeer.GetStatus()} status. Reason: {reason}");
                OnDisconnected?.Invoke();
            };
        }

        public void OnStatusChanged(ShamanClientStatus prevStatus, ShamanClientStatus newStatus)
        {
            Logger.Info($"Connect status: prev: {prevStatus} new {newStatus}");    
            ConnectionStatus = new ShamanConnectionStatus(newStatus);
        }
        
        public Guid RegisterEventHandler<T>(Action<T> handler, bool callOnce = false)
            where T:MessageBase, new()
        {
            return ClientPeer.RegisterOperationHandler<T>(handler, callOnce);
        }

        public void UnregisterOperationHandler(Guid handlerId)
        {
            ClientPeer.UnregisterOperationHandler(handlerId);
        }
    
        public void SendEvent(EventBase eve)
        {
            lock (_queueSync)
            {
                if (ConnectionStatus != null && ConnectionStatus.Status == ShamanClientStatus.InRoom)
                {
                    ClientPeer.SendEvent(eve);
                }
                else
                {
                    Logger.Error($"SendEvent error: Wrong connection status");
                }
            }
        }

        public void Disconnect()
        {
            _isSelfDisconnect = true;
            ClientPeer.Disconnect();
        }

        public void SendRequest<T>(HttpRequestBase request, Action<T> callback, string url = null) where T : HttpResponseBase, new()
        {
            throw new NotImplementedException();
        }

        public async Task<T> SendRequest<T>(HttpRequestBase request, string url = null) where T:HttpResponseBase, new()
        {
            request.SessionId = SessionId;
            var r =  await ClientPeer.SendWebRequest<T>(url ?? GetBackendAddress(), request);
            var responseWithPlayer = r as ResponseWithPlayer;
            if (responseWithPlayer?.Player != null)
            {
                if (responseWithPlayer.Success && StorageContainer != null && StorageContainer.IsReadyForRequests())
                {
                    StorageContainer.GetStorage().SetPlayerDelta(responseWithPlayer.Player);
                    StorageContainer.GetStorage().SetPlayerStaticData();
                    _guestId = responseWithPlayer.Player.GuestId;
                    OnPlayerUpdatedEvent?.Invoke(StorageContainer.GetStorage().Player);
                    LastPlayerUpdateTime = DateTime.UtcNow;
                }
            }

            return r;
        }
        
        public async Task<T> SendRequest<T>(RequestBase request) where T:ResponseBase, new()
        {
            return await ClientPeer.SendRequest<T>(request);
        }
        
        private string GetBackendAddress()
        {
            if (_route == null)
                throw new Exception("_route is null");

            return $"{_route.BackendProtocol}://{_route.BackendAddress}:{_route.BackendPort}";
        }

        private async void OnAuthTokenReceived(GetAuthTokenResponse response)
        {
            if (response.ResultCode != ResultCode.OK)
            {
                Logger.Error($"AuthToken get error: {response.Message}");
                OnConnectionFailed?.Invoke();
                return;
            }

            _authToken = response.AuthToken;
        
            Logger.Info($"Sending InitializationRequest");
            var result = await SendRequest<InitializationResponse>(new InitializationRequest(_authToken, _guestId, new Dictionary<int, string>()));
            OmInitializationResponse(result);
        }

        private void OmInitializationResponse(InitializationResponse response)
        {
            if (response.ResultCode != ResultCode.OK)
            {
                Logger.Error($"Initialization error: {response.Message}");
                OnConnectionFailed?.Invoke();
                return;
            }

            SessionId = response.SessionId;

            if (Storage == null)
            {
                Logger.Error($"Storage is null inside InitializationResponse handler");
                return;
            }
        
            Storage.SetPlayerDelta(response.Player);

            StorageContainer.Start("");
        
            OnConnected?.Invoke();
        }

        private void OnRoutesReceived(List<Route> routes)
        {
            if (routes == null || routes.Count == 0)
            {
                Logger.Error($"No matchmakers received");
                OnGameNeedToBeUpdated?.Invoke();
                return;
            }
            else
                _route = routes.FirstOrDefault();

            //start storage updater
            Logger.Info($"Starting storage container");
            ((ClientStorageContainer)StorageContainer).Initialize(GetBackendAddress());
       
            //get auth token after storage updated
            StorageContainer.SubscribeOnStorageUpdated(async (status) => 
            {
                Logger.Info($"Storage container changed status: {status}");
                if (status == StorageContainerStatus.Updated)
                {
                    Logger.Info($"Sending GetAuthTokenRequest");
                    var response = await SendRequest<GetAuthTokenResponse>(new GetAuthTokenRequest());
                    OnAuthTokenReceived(response);
                }
                OnStorageStatusChanged?.Invoke(status);
            });
        
            ((ClientStorageContainer)StorageContainer).UpdateOnce();
        }

    
        public async Task Connect()
        {
            var response = await ServerProvider.GetRoutes(NetworkConfiguration.RouterUrl, NetworkConfiguration.ClientVersion);
            OnRoutesReceived(response);
        }
    
        public async Task<JoinInfo> JoinGame(Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties)
        {
            if (matchMakingProperties == null)
                matchMakingProperties = new Dictionary<byte, object>();
        
            //adding backend
            joinGameProperties.Add(PropertyCode.PlayerProperties.BackendId, _route.BackendId);
        
            //reset manual disconnect flag
            _isSelfDisconnect = false;

            return await ClientPeer.JoinGame(_route.MatchMakerAddress, _route.MatchMakerPort, _route.BackendId, SessionId, 
                matchMakingProperties, joinGameProperties);
        }
        
        public async Task<JoinInfo> DirectJoinGame(string gameServerAddress, ushort gameServerPort, Guid roomId)
        {
            //reset manual disconnect flag
            _isSelfDisconnect = false;

            var joinGameProperties = new Dictionary<byte, object> {{PropertyCode.PlayerProperties.BackendId, _route.BackendId}};
        
            return await ClientPeer.DirectConnectToGameServer(gameServerAddress, gameServerPort, SessionId, roomId, joinGameProperties);
        }
        
        public async Task<JoinInfo> DirectJoinGame(string gameServerAddress, ushort gameServerPort, ushort httpPort)
        {
            var roomId = Guid.NewGuid();
            
            //reset manual disconnect flag
            _isSelfDisconnect = false;

            var joinGameProperties = new Dictionary<byte, object> {{PropertyCode.PlayerProperties.BackendId, 1}};
            
            var response = await SendRequest<CreateRoomResponse>(new CreateRoomRequest(
                new Dictionary<byte, object>
                {
                    {PropertyCode.RoomProperties.GameMode, (byte) GameMode.TeamPlay},
                    {PropertyCode.RoomProperties.MatchMakerUrl, "http://nahui.com"}
                }, new Dictionary<Guid, Dictionary<byte, object>>
                {
                    {SessionId, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.GameMode, (byte) GameMode.TeamPlay}}},
                }) {RoomId = roomId}, $"http://{gameServerAddress}:{httpPort}");
            
            
            return await ClientPeer.DirectConnectToGameServer(gameServerAddress, gameServerPort, SessionId, response.RoomId, joinGameProperties);
        }

        public int GetPing()
        {
            return ClientPeer.GetPing();
        }
        
        private void FixedUpdate()
        {
            
            if (Time.time - _lastReceivedOn > 1 / _receiveRatePerSec)
            {
                try
                {
                    if (ClientPeer.GetStatus() == ShamanClientStatus.InRoom)
                        Logger.Debug($"Start processing messages. (Current queue size {ClientPeer.GetMessagesCountInQueue()}, CurrentPing {ClientPeer.GetPing()})");
                    ClientPeer.ProcessMessages();
                }
                catch (Exception e)
                {
                    Logger.Error($"Process messages error: {e}");
                }
                finally
                {
                    _lastReceivedOn = Time.time;
                }
            }
            
            if (Time.time - _lastQueuesProcessedOn > (float)1 / _syncersProcessQueuesIntervalMs)
            {
                _lastQueuesProcessedOn = Time.time;
            }
        }
    
        private void OnDestroy()
        {
            if (ClientPeer.GetStatus() != ShamanClientStatus.Offline)
                ClientPeer?.Disconnect();
        }
    }
}
