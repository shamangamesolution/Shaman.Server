using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Shaman.Common.Server.Peers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common.Logging;
using Shaman.LiteNetLibAdapter;
using Shaman.MM.MatchMaking;
using Shaman.MM.Peers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.MM
{
    public class MmPeerListener : PeerListenerBase<MmPeer>
    {
        private IMatchMaker _matchMaker;
        private IShamanMessageSender _messageSender;
        private IRoomManager _roomManager;
        private IMatchMakingGroupsManager _matchMakingGroupsManager;
        private string _authSecret;

        public void Initialize(IMatchMaker matchMaker, IShamanMessageSender packetSender, IRoomManager roomManager, IMatchMakingGroupsManager matchMakingGroupsManager,
            string authSecret)
        {
            _matchMaker = matchMaker;
            _messageSender = packetSender;
            _authSecret = authSecret;
            _roomManager = roomManager;
            _matchMakingGroupsManager = matchMakingGroupsManager;
        }

        private JoinInfo GetJoinInfo(JoinRoomResult joinResult)
        {
            var room = _roomManager.GetRoom(joinResult.RoomId);
            if (room == null)
                throw new Exception($"CreateRoomFromClient error: room {joinResult.RoomId} is not exists");
            switch (joinResult.Result)
            {
                case RoomOperationResult.OK:
                    return new JoinInfo(joinResult.Address, joinResult.Port, joinResult.RoomId, JoinStatus.RoomIsReady,
                        room.CurrentPlayersCount, room.TotalPlayersNeeded, true);
                case RoomOperationResult.ServerNotFound:
                case RoomOperationResult.CreateRoomError:
                case RoomOperationResult.JoinRoomError:
                    _logger.Error($"DirectJoin error: JoinResult = {joinResult.Result},");
                    return new JoinInfo("", 0, Guid.Empty, JoinStatus.MatchMakingFailed, 0, 0);
                default:
                    throw new ArgumentException();
            }
        }

        private void ProcessMessage(IPEndPoint endPoint, byte[] buffer, int offset, int length, MmPeer peer)
        {
            //probably bad kind of using
            var operationCode = MessageBase.GetOperationCode(buffer, offset);
            _logger.Debug($"Message received. Operation code: {operationCode}");

            peer = PeerCollection.Get(endPoint);
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
                    //ping processing
                    break;
                case ShamanOperationCode.Disconnect:
                    OnClientDisconnect(endPoint, new LightNetDisconnectInfo(ClientDisconnectReason.PeerLeave));
                    break;
                case ShamanOperationCode.Authorization:
                    var authMessage =
                        Serializer.DeserializeAs<AuthorizationRequest>(buffer, offset, length);
                    
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

                    switch (operationCode)
                    {
                        case ShamanOperationCode.PingRequest:
                            _messageSender.Send(new PingResponse(), peer);
                            break;
                        case ShamanOperationCode.CreateRoomFromClient:
                            var createRequest =
                                Serializer.DeserializeAs<CreateRoomFromClientRequest>(buffer, offset, length);
                            TaskScheduler.ScheduleOnceOnNow(async () =>
                            {
                                var createResult = await _matchMakingGroupsManager.CreateRoom(peer.GetSessionId(),
                                    createRequest.MatchMakingProperties);
                                //parse create result and create response
                                var createResponse = new CreateRoomFromClientResponse(GetJoinInfo(createResult));
                                if (createResult.Result == RoomOperationResult.OK)
                                    _logger.Info($"Room {createResult.RoomId} created");
                                _messageSender.Send(createResponse, peer);
                            });
                            break;
                        case ShamanOperationCode.DirectJoin:
                            var joinRequest = Serializer.DeserializeAs<DirectJoinRequest>(buffer, offset, length);
                            TaskScheduler.ScheduleOnceOnNow(async () =>
                            {
                                //join existing room
                                var joinResult = await _roomManager.JoinRoom(joinRequest.RoomId,
                                    new Dictionary<Guid, Dictionary<byte, object>>
                                        {{ peer.GetSessionId(), joinRequest.MatchMakingProperties}});
                                //parse join result and create response
                                var joinResponse = new DirectJoinResponse(GetJoinInfo(joinResult));
                                if (joinResult.Result == RoomOperationResult.OK)
                                    _logger.Info($"Player direct joined room {joinResult.RoomId}");
                                _messageSender.Send(joinResponse, peer);
                            });
                            break;
                        case ShamanOperationCode.GetRoomList:
                            var request = Serializer.DeserializeAs<GetRoomListRequest>(buffer, offset, length);
                            var rooms = _matchMakingGroupsManager.GetRooms(request.MatchMakingProperties)
                                .Select(r => new RoomInfo(r.Id, r.TotalPlayersNeeded, r.CurrentPlayersCount, r.Properties, r.State))
                                .ToList();
                            var getRoomsResponse = new GetRoomListResponse();
                            getRoomsResponse.Rooms = rooms;
                            _messageSender.Send(getRoomsResponse, peer);
                            break;
                        case ShamanOperationCode.EnterMatchMaking:
                            var enterMessage =  Serializer.DeserializeAs<EnterMatchMakingRequest>(buffer, offset, length);
                            if (enterMessage == null)
                                throw new Exception("Can not deserialize EnterMatchMakingRequest. Result is null");
                                                            
                            
                            var properties = enterMessage.MatchMakingProperties;

                            var requiredProperties = _matchMaker.GetRequiredProperties();

                            foreach (var property in requiredProperties)
                            {
                                if (!properties.ContainsKey(property))
                                {
                                    _logger.Error($"Player {peer.GetPeerId()} tried to enter matchmaking without property {property}");
                                    _messageSender.Send(new EnterMatchMakingResponse(MatchMakingErrorCode.RequiredPlayerPropertyIsNotSet), peer);
                                    return;
                                }
                            }
                            
                            //add player
                            _matchMaker.AddPlayer(peer, enterMessage.MatchMakingProperties);
                            
                            _messageSender.Send(new EnterMatchMakingResponse(), peer);

                            break;
                        case ShamanOperationCode.LeaveMatchMaking:
                            //remove player
                            _matchMaker.RemovePlayer(peer.GetPeerId());
                            //send response
                            _messageSender.Send(new LeaveMatchMakingResponse(), peer);
                            break;
                        default:
                            _messageSender.Send(new ErrorResponse(ResultCode.UnknownOperation), peer);
                            return;
                    }

                    break;
            }
        }

        public override void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket)
        {
            MmPeer peer = null;
            try
            {
                peer = PeerCollection.Get(endPoint);
                var offsets = PacketInfo.GetOffsetInfo(dataPacket.Buffer, dataPacket.Offset);
                foreach (var item in offsets)
                {
                    try
                    {
                        ProcessMessage(endPoint, dataPacket.Buffer, item.Offset, item.Length, peer);
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
                _logger.Error($"Error processing message: {ex}");
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

        protected override void ProcessDisconnectedPeer(MmPeer peer, IDisconnectInfo info)
        {
            _matchMaker.RemovePlayer(peer.GetPeerId());
            _messageSender.CleanupPeerData(peer);
        }
    }
}