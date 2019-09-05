using System;
using System.Net;
using Shaman.Common.Server.Peers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Sockets;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests.Auth;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.DTO.Responses.Auth;

namespace Shaman.Game
{
    public class GamePeerListener : PeerListenerBase<GamePeer>
    {
        private IRoomManager _roomManager;
        private IBackendProvider _backendProvider;
        private IPacketSender _packetSender;
        
        public void Initialize(IRoomManager roomManager, IBackendProvider backendProvider, IPacketSender packetSender)
        {
            _roomManager = roomManager;
            _backendProvider = backendProvider;
            _packetSender = packetSender;
        }

        private void ProcessMessage(PacketInfo obj, int offset, int length, GamePeer peer)
        {
            var endPoint = obj.EndPoint;
            var message = new ArraySegment<byte>(obj.Buffer, offset, length).ToArray();
            //recycle buffer
            //obj.RecycleCallback?.Invoke();
            var operationCode = MessageBase.GetOperationCode(message);
            _logger.Debug($"Message received. Operation code: {operationCode}");

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
                    _packetSender.AddPacket(new PingEvent(), peer);
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
                        
                        peer.IsAuthorizing = true;

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
                        default:
                            _roomManager.ProcessMessage(
                                MessageFactory.DeserializeMessage(operationCode, SerializerFactory, message),
                                peer);
                            break;
                    }

                    break;
            }
        }
        
        public override void OnReceivePacketFromClient(PacketInfo obj)
        {
            GamePeer peer = null;
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
            
            if (_roomManager.IsInRoom(peer.GetSessionId()))
                _roomManager.PeerDisconnected(peer.GetSessionId());
            
            base.OnClientDisconnect(endPoint, reason);            
        }
    }
}