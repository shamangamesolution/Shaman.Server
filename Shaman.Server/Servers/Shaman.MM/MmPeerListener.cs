using System;
using System.Net;
using Shaman.Common.Server.Peers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Sockets;
using Shaman.MM.MatchMaking;
using Shaman.MM.Peers;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests.Auth;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.DTO.Responses.Auth;
using Shaman.Messages.MM;

namespace Shaman.MM
{
    public class MmPeerListener : PeerListenerBase<MmPeer>
    {
        private IMatchMaker _matchMaker;
        private IBackendProvider _backendProvider;
        private IPacketSender _packetSender;
        
        public void Initialize(IMatchMaker matchMaker, IBackendProvider backendProvider, IPacketSender packetSender)
        {
            _matchMaker = matchMaker;
            _backendProvider = backendProvider;
            _packetSender = packetSender;
        }

        private void ProcessMessage(PacketInfo obj, int offset, int length, MmPeer peer)
        {
            //probably bad kind of using
            var endPoint = obj.EndPoint;
            var message = new ArraySegment<byte>(obj.Buffer, offset, length).ToArray();
            var operationCode = MessageBase.GetOperationCode(message);
            _logger.Debug($"Message received. Operation code: {operationCode}");

            peer = PeerCollection.Get(endPoint);
            if (peer == null)
            {
                _logger.Error(
                    $"GamePeerListener.OnReceivePacketFromClient error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }


            //listener handles only auth message, others are sent to roomManager
            switch (operationCode)
            {
                case CustomOperationCode.Connect:
                    _packetSender.AddPacket(new ConnectedEvent(), peer);
                    break;
                case CustomOperationCode.Ping:
                    //ping processing
                    break;
                case CustomOperationCode.Disconnect:
                    OnClientDisconnect(endPoint, "On Disconnect event received");
                    break;
                case CustomOperationCode.Authorization:
                    var authMessage =
                        MessageBase.DeserializeAs<AuthorizationRequest>(SerializerFactory, message);
                    
                    if (!Config.IsAuthOn())
                    {
                        //if success - send auth success
                        peer.IsAuthorizing = false;
                        peer.IsAuthorized = true;
                        //this sessionID will be got from backend, after we send authToken, which will come in player properties
                        peer.SetSessionId(authMessage.SessionId);
                        _packetSender.AddPacket(new AuthorizationResponse(), peer);
                    }
                    else
                    {
                        if (peer.IsAuthorizing)
                            return;

                        RequestSender.SendRequest<ValidateSessionIdResponse>(
                            _backendProvider.GetBackendUrl(authMessage.BackendId),
                            new ValidateSessionIdRequest(authMessage.SessionId, "secret"),
                            (response) =>
                            {
                                if (response.ResultCode == ResultCode.OK)
                                {
                                    //if success - send auth success
                                    peer.IsAuthorizing = false;
                                    peer.IsAuthorized = true;
                                    //this sessionID will be got from backend, after we send authToken, which will come in player properties
                                    peer.SetSessionId(authMessage.SessionId);
                                    _packetSender.AddPacket(new AuthorizationResponse(), peer);
                                }
                                else
                                {
                                    peer.IsAuthorizing = false;
                                    _packetSender.AddPacket(new AuthorizationResponse() {ResultCode = ResultCode.NotAuthorized}, peer);
                                }
                            });
                    }
                    break;
                default:
                    if (!peer.IsAuthorized && Config.IsAuthOn())
                    {
                        _packetSender.AddPacket(new AuthorizationResponse() {ResultCode = ResultCode.NotAuthorized}, peer);
                        return;
                    }

                    switch (operationCode)
                    {
                        case CustomOperationCode.EnterMatchMaking:
                            var enterMessage =  MessageBase.DeserializeAs<EnterMatchMakingRequest>(SerializerFactory, message);
                            if (enterMessage == null)
                                throw new Exception("Can not deserialize EnterMatchMakingRequest. Result is null");
                                                            
                            
                            var properties = enterMessage.MatchMakingProperties;

                            var requiredProperties = _matchMaker.GetRequiredProperties();

                            foreach (var property in requiredProperties)
                            {
                                if (!properties.ContainsKey(property))
                                {
                                    _logger.Error($"Player {peer.GetPeerId()} tried to enter matchmaking without property {property}");
                                    _packetSender.AddPacket(new EnterMatchMakingResponse(MatchMakingErrorCode.RequiredPlayerPropertyIsNotSet), peer);
                                    return;
                                }
                            }
                            
                            //add player
                            _matchMaker.AddPlayer(peer, enterMessage.MatchMakingProperties);
                            
                            _packetSender.AddPacket(new EnterMatchMakingResponse(), peer);

                            break;
                        case CustomOperationCode.LeaveMatchMaking:
                            //remove player
                            _matchMaker.RemovePlayer(peer.GetPeerId());
                            //send response
                            _packetSender.AddPacket(new LeaveMatchMakingResponse(), peer);
                            break;
                        default:
                            _packetSender.AddPacket(new ErrorResponse(ResultCode.UnknownOperation), peer);
                            return;
                    }

                    break;
            }
        }

        public override void OnReceivePacketFromClient(PacketInfo obj)
        {
            MmPeer peer = null;
            try
            {
                var endPoint = obj.EndPoint;
                peer = PeerCollection.Get(endPoint);
                var offsets = PacketInfo.GetOffsetInfo(obj.Buffer, obj.Offset);
                foreach (var item in offsets)
                {
                    try
                    {
                        ProcessMessage(obj, item.Offset, item.Length, peer);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error processing message: {ex}");
                        if (peer != null)
                            _packetSender.AddPacket(new ErrorResponse(ResultCode.MessageProcessingError), peer);
                    }
                }
                obj.RecycleCallback?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing message: {ex}");
                if (peer != null)
                    _packetSender.AddPacket(new ErrorResponse(ResultCode.MessageProcessingError), peer);
            }

        }
        
        public override void OnNewClientConnect(IPEndPoint endPoint)
        {
            base.OnNewClientConnect(endPoint);
            
            var peer = PeerCollection.Get(endPoint);
            if (peer == null)
            {
                _logger.Error($"GamePeerListener.OnClientDisconnect error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }
            
            _packetSender.AddPacket(new ConnectedEvent(), peer);
        }
        
        public override void OnClientDisconnect(IPEndPoint endPoint, string reason)
        {
            var peer = PeerCollection.Get(endPoint);
            if (peer == null)
            {
                _logger.Error($"GamePeerListener.OnClientDisconnect error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }
            
            _matchMaker.RemovePlayer(peer.GetPeerId());
            
            base.OnClientDisconnect(endPoint, reason);            
        }
    }
}