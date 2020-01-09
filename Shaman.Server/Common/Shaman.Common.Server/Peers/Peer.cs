using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Server.Peers
{
    public class Peer : IPeer
    {
        private IPEndPoint _endpoint;
        private Guid _peerId;
        private IReliableSock _socket;
        private Guid _sessionId;

        public Guid GetPeerId()
        {
            return _peerId;
        }

        public void SetSessionId(Guid sessionId)
        {
            this._sessionId = sessionId;
        }

        public Guid GetSessionId()
        {
            return _sessionId;
        }

        public void Initialize(IPEndPoint endpoint, Guid peerId, IReliableSock socket, ISerializer serializer, IApplicationConfig config, IShamanLogger logger)
        {
            _endpoint = endpoint;
            _peerId = peerId;
            _socket = socket;
        }

        public void Disconnect(DisconnectReason reason)
        {
            _socket.Close();
        }

//        public void Send(MessageBase message)
//        {
//            _logger.Debug($"Sending {message.GetType()} message to peer {GetPeerId()}");
//            var initMsgArray = message.Serialize(_serializerFactory);
////            var buf = _bufferPool.Get(initMsgArray.Length);
////            Array.Copy(initMsgArray, 0, buf, 0, initMsgArray.Length);
//            _socket.Send(_endpoint, initMsgArray, 0, initMsgArray.Length, message.IsReliable, message.IsOrdered);
//        }

        public void Send(PacketInfo packetInfo)
        {
            _socket.Send(_endpoint, packetInfo.Buffer, packetInfo.Offset, packetInfo.Length,
                packetInfo.IsReliable, packetInfo.IsOrdered);
            packetInfo.Dispose();
        }
        
//        public void Send(byte[] bytes, bool isReliable, bool isOrdered)
//        {
//            _socket.Send(_endpoint, bytes, 0, bytes.Length, isReliable, isOrdered);
//        }
    }
}