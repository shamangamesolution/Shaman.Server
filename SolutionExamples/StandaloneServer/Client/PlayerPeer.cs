using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Messages;
using Shaman.Client.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.RoomFlow;

namespace Client
{
    public interface IMessageHandler
    {
        void OnIncoming(PlayerPeer playerPeer, ISerializer serializer, ushort operationCode, byte[] packetBuffer,
            int offset, int length);
        
        void OnJoined(PlayerPeer peer, ISerializer serializer);
    }

    public class PlayerPeer
    {
        private readonly string _serverHost;
        private readonly int _serverPort;
        private readonly ClientPeer _peer;
        private readonly ISerializer _serializer;
        private Guid _roomId;
        public Guid PlayerId { get; }

        public PlayerPeer(string serverHost, int serverPort)
        {
            _serverHost = serverHost;
            _serverPort = serverPort;
            _peer = CreatePeerClient();
            PlayerId = Guid.NewGuid();
            _serializer = new BinarySerializer();
        }

        public void RegisterMessageHandler(IMessageHandler handler)
        {
            _peer.OnPackageAvailable = () =>
            {
                IPacketInfo packet;
                while ((packet = _peer.PopNextPacket()) != null)
                {
                    foreach (var offsetInfo in PacketInfo.GetOffsetInfo(packet.Buffer, packet.Offset))
                    {
                        var operationCode = MessageBase.GetOperationCode(packet.Buffer, offsetInfo.Offset);

                        if (operationCode == CustomOperationCode.AuthorizationResponse)
                        {
                            _peer.Send(new JoinRoomRequest(_roomId, new Dictionary<byte, object>()));
                        }
                        
                        if (operationCode == MessageCodes.PlayerJoinedEvent)
                        {
                            handler.OnJoined(this, _serializer);
                        }

                        if (operationCode == CustomOperationCode.Connect)
                        {
                            _peer.Send(new AuthorizationRequest(PlayerId));
                        }

                        handler.OnIncoming(this, _serializer, operationCode, packet.Buffer, offsetInfo.Offset,
                            offsetInfo.Length);
                        packet.Dispose();
                    }
                }
            };
        }

        public void Send(MessageBase message)
        {
            _peer.Send(message);
        }

        public void JoinRoom(Guid roomId)
        {
            _roomId = roomId;
            _peer.Connect(_serverHost, _serverPort);
        }

        private static ClientPeer CreatePeerClient()
        {
            return new ClientPeer(new ConsoleLogger(), new TaskSchedulerFactory(new ConsoleLogger()), 300, 33);
        }
    }
}