using System;
using System.Net;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using Shaman.Game.Contract;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.LiteNetLibAdapter;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests.Auth;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.DTO.Responses.Auth;

namespace Shaman.Game
{
    public class GamePeerListener : PeerListenerBase<GamePeer>
    {
        private IRoomManager _roomManager;
        private IPacketSender _packetSender;
        private string _authSecret;

        public void Initialize(IRoomManager roomManager, IPacketSender packetSender,
            string authSecret)
        {
            _roomManager = roomManager;
            _packetSender = packetSender;
            _authSecret = authSecret;
        }

        private void ProcessMessage(IPEndPoint endPoint, MessageData messageData,
            GamePeer peer)
        {
            var operationCode = MessageBase.GetOperationCode(messageData.Buffer, messageData.Offset);
            _logger.Debug($"Message received. Operation code: {operationCode}");

            if (peer == null)
            {
                _logger.Warning($"GamePeerListener.OnReceivePacketFromClient error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
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
                    OnClientDisconnect(endPoint, new LightNetDisconnectInfo(ClientDisconnectReason.PeerLeave));
                    break;
                case CustomOperationCode.Authorization:
                    var authMessage = Serializer.DeserializeAs<AuthorizationRequest>(messageData.Buffer, messageData.Offset, messageData.Length);
                    
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
                        //TODO authorizing logic
                        throw new NotImplementedException();
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
                            _roomManager.ProcessMessage(operationCode, messageData, peer);
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
                        var messageData = new MessageData(dataPacket.Buffer, item.Offset, item.Length, dataPacket.IsReliable);
                        ProcessMessage(endPoint, messageData, peer);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error processing message: {ex}");
                        if (peer != null)
                            _packetSender.AddPacket(new ErrorResponse(ResultCode.MessageProcessingError), peer);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"OnReceivePacketFromClient: Error processing package: {ex}");
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
                _logger.Warning($"GamePeerListener.OnClientDisconnect error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }
            
            _packetSender.AddPacket(new ConnectedEvent(), peer);
        }

        protected override void ProcessDisconnectedPeer(GamePeer peer, IDisconnectInfo info)
        {
            if (_roomManager.IsInRoom(peer.GetSessionId()))
                _roomManager.PeerDisconnected(peer, info);

            _packetSender.PeerDisconnected(peer);
        }
    }
}