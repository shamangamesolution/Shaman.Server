using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Shaman.Client.Peers;
using Shaman.Client.Providers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.TestTools.ClientPeers
{
    public class TestClientPeer: IDisposable
    {
        private readonly ClientPeer _clientPeer;
        private readonly IShamanLogger _logger;
        private readonly object _syncCollection = new object();
        private List<MessageBase> _receivedMessages = new List<MessageBase>();
        private JoinInfo _joinInfo;
        private readonly ITaskScheduler _taskScheduler;

        public Guid SessionId { get; set; }

        private readonly List<Route> _routeTable = new List<Route>();

        public string MmAddress => _routeTable.First().MatchMakerAddress;
        public ushort MmPort => _routeTable.First().MatchMakerPort;
        public string BackendUrl => $"{_routeTable.First().BackendProtocol}://{_routeTable.First().BackendAddress}:{_routeTable.First().BackendPort}/";
        public int BackendId => _routeTable.First().BackendId;
        public Action<string> OnDisconnectedFromServer;

        public TestClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory)
        {
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _clientPeer = new ClientPeer(logger,taskSchedulerFactory, 300, 20);
            //_clientPeer.OnPackageReceived += ClientOnPackageReceived;

            _clientPeer.OnDisconnectedFromServer = OnDisconnected;
            
            _taskScheduler.ScheduleOnInterval(() =>
            {
                IPacketInfo pack = null;
                while ((pack = _clientPeer.PopNextPacket()) != null)
                {
                    ClientOnPackageReceived(pack);
                }
            }, 0, 20);
        }

        private void OnDisconnected(string reason)
        {
            Console.Out.WriteLine("Disconnected from server: {0}", reason);
            OnDisconnectedFromServer?.Invoke(reason);
        }


        public async Task LoadRoutes(string routerUrl, string clientVersion)
        {
            var httpSender = new HttpSender(_logger, new BinarySerializer());
            var clientServerInfoProvider = new ClientServerInfoProvider(httpSender, _logger);

            await clientServerInfoProvider.GetRoutes(routerUrl, clientVersion, list => _routeTable.AddRange(list));
            _routeTable.Should().NotBeEmpty();
        }

        public JoinInfo GetJoinInfo()
        {
            return _joinInfo;
        }
        
        public void Connect(string address, ushort port)
        {
            _clientPeer.Connect(address, port);
        }

        public void Disconnect()
        {
            _clientPeer.Disconnect();
        }

        public List<MessageBase> GetMessageList()
        {
            return _receivedMessages;
        }
        public void ClearMessages()
        {
            _receivedMessages = new List<MessageBase>();
        }

        public int GetCountOf(ushort operationCode)
        {
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode);
            return count;
        }
        
        public int GetCountOfSuccessResponses(ushort operationCode)
        {
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode && m.Type == MessageType.Response && ((ResponseBase)m).ResultCode == ResultCode.OK);
            return count;
        }
        public int GetCountOfNotSuccessResponses(ushort operationCode)
        {
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode && m.Type == MessageType.Response && ((ResponseBase)m).ResultCode != ResultCode.OK);
            return count;
        }
        public void Send(MessageBase message)
        {
            _clientPeer.Send(message);
        }

        private void ProcessMessage(byte[] buffer, int offset, int length)
        {
            var bitFac = new BinarySerializer();
            var operationCode = MessageBase.GetOperationCode(buffer, offset);
            _logger.Info($"Message received. Operation code: {operationCode}");
            var deserialized = MessageFactory.DeserializeMessageForTest(operationCode, bitFac, buffer,offset, length);

            _receivedMessages.Add(deserialized);

            //save join info
            if (deserialized is JoinInfoEvent joinInfoEvent)
            {
                _joinInfo = joinInfoEvent.JoinInfo;
                _logger.Info($"Received joinInfo. Status = {_joinInfo.Status}");
            }
        }
        
        private void ClientOnPackageReceived(IPacketInfo packet)
        {
            lock (_syncCollection)
            {
                var offsets = PacketInfo.GetOffsetInfo(packet.Buffer, packet.Offset);
                foreach (var item in offsets)
                {
                    try
                    {
                        ProcessMessage(packet.Buffer, item.Offset, item.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error processing message: {ex}");
                    }
                }
                packet.Dispose();

            }
        }

        public bool IsConnected()
        {
            return _clientPeer.IsConnected();
        }

        public async Task<T> WaitFor<T>(Func<T, bool> condition,
            int timeoutMs = 10000, int checkPeriod = 10) where T : MessageBase
        {
            var stopwatch = Stopwatch.StartNew();
            T msg = null;
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                msg = (T) _receivedMessages.LastOrDefault(m =>
                    m.GetType() == typeof(T));

                if (msg != null && condition(msg))
                {
                    Console.WriteLine($"Condition for event {typeof(T)} was matched for {stopwatch.ElapsedMilliseconds}ms");
                    return msg;
                }

                await Task.Delay(checkPeriod);
            }

            throw new TestClientPeerException(this,
                msg == null
                    ? $"Event of type {typeof(T)} was not received for {stopwatch.ElapsedMilliseconds}ms."
                    : $"Event of type {typeof(T)} with specified condition was not received for {stopwatch.ElapsedMilliseconds}ms.");
        }

        public async Task<TimeSpan> Ping()
        {
            ClearMessages();
            var stopwatch = Stopwatch.StartNew();
            var request = new PingRequest();
            Send(request);
            await WaitFor((PingResponse response) => response.ResultCode == ResultCode.OK && response.SourceTicks == request.SourceTicks, 2000);
            return stopwatch.Elapsed;
        }

        public void Dispose()
        {
            _taskScheduler.Dispose();
        }
    }
}