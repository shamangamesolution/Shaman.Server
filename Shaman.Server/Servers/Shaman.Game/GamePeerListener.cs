using System;
using System.Net;
using Shaman.Common.Server.Peers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.LiteNetLibAdapter;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Game
{
    public class GamePeerListener : PeerListenerBase<GamePeer>
    {
        private IRoomManager _roomManager;
        private IShamanMessageSender _messageSender;
        private string _authSecret;

        public void Initialize(IRoomManager roomManager, IShamanMessageSender messageSender,
            string authSecret)
        {
            _roomManager = roomManager;
            _messageSender = messageSender;
            _authSecret = authSecret;
        }

        private void ProcessMessage(IPEndPoint endPoint, Payload payload,
            DeliveryOptions deliveryOptions,
            GamePeer peer)
        {
            var operationCode = MessageBase.GetOperationCode(payload.Buffer, payload.Offset);
            _logger.Debug($"Message received. Operation code: {operationCode}");

            if (peer == null)
            {
                _logger.Warning($"GamePeerListener.OnReceivePacketFromClient error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }
            
            //listener handles only auth message, others are sent to roomManager
            switch (operationCode)
            {
                case ShamanOperationCode.Connect:
                    _messageSender.Send(new ConnectedEvent(), peer);
                    break;
                case ShamanOperationCode.Ping:
                    _messageSender.Send(new PingEvent(), peer);
                    break;
                case ShamanOperationCode.Disconnect:
                    OnClientDisconnect(endPoint, new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave));
                    break;
                case ShamanOperationCode.Authorization:
                    var authMessage = Serializer.DeserializeAs<AuthorizationRequest>(payload.Buffer, payload.Offset, payload.Length);
                    
                    if (!Config.IsAuthOn)
                    {
                        //if success - send auth success
                        peer.IsAuthorizing = false;
                        peer.IsAuthorized = true;
                        //this sessionID will be got from backend, after we send authToken, which will come in player properties
                        peer.SetSessionId(authMessage.SessionId);
                        _messageSender.Send(new AuthorizationResponse(), peer);
                    }
                    else
                    {
                        //TODO authorizing logic
                        throw new NotImplementedException();
                    }
                    break;
                default:
                    if (!peer.IsAuthorized && Config.IsAuthOn)
                    {
                        _messageSender.Send(new AuthorizationResponse() {ResultCode = ResultCode.NotAuthorized}, peer);
                        return;
                    }
                    else
                    {
                        if (!peer.IsAuthorized)
                            _messageSender.Send(new AuthorizationResponse(), peer);
                    }

                    switch (operationCode)
                    {
                        default:
                            _roomManager.ProcessMessage(operationCode, payload, deliveryOptions, peer);
                            break;
                    }

                    break;
            }
        }
        
        public override void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket)
        {
            GamePeer peer = null;
            try
            {
                peer = PeerCollection.Get(endPoint);
                var offsets = PacketInfo.GetOffsetInfo(dataPacket.Buffer, dataPacket.Offset);
                foreach (var item in offsets)
                {
                    try
                    {
                        var messageData = new Payload(dataPacket.Buffer, item.Offset, item.Length);
                        ProcessMessage(endPoint, messageData, dataPacket.DeliveryOptions, peer);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error processing message: {ex}");
                        if (peer != null)
                            _messageSender.Send(new ErrorResponse(ResultCode.MessageProcessingError), peer);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"OnReceivePacketFromClient: Error processing package: {ex}");
                if (peer != null)
                    _messageSender.Send(new ErrorResponse(ResultCode.MessageProcessingError), peer);
            }
        }

        public override void OnNewClientConnect(IPEndPoint endPoint)
        {
            base.OnNewClientConnect(endPoint);
            
            var peer = PeerCollection.Get(endPoint);
            if (peer == null)
            {
                _logger.Warning($"GamePeerListener.OnClientDisconnect error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }
            
            _messageSender.Send(new ConnectedEvent(), peer);
        }

        protected override void ProcessDisconnectedPeer(GamePeer peer, IDisconnectInfo info)
        {
            try
            {
                if (_roomManager.IsInRoom(peer.GetSessionId()))
                    _roomManager.PeerDisconnected(peer, info);
            }
            finally
            {
                _messageSender.CleanupPeerData(peer);
            }
        }
    }
}