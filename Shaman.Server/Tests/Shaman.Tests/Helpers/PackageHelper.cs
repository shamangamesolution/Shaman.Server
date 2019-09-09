using System;
using System.Net;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Tests.Helpers
{
    public class PackageHelper
    {
        public static PacketInfo GetPacketInfo(MessageBase message, IShamanLogger logger)
        {
            var _serializerFactory = new SerializerFactory(logger);
            _serializerFactory.InitializeDefaultSerializers(8, "");
            var initMsgArray = message.Serialize(_serializerFactory);
//            var buf = _buffer.Get(initMsgArray.Length, "ForMessage");
//            Array.Copy(initMsgArray, 0, buf, 0, initMsgArray.Length);
            PacketInfo info = new PacketInfo(300);
            info.Add(initMsgArray, message.IsReliable, message.IsOrdered);
            info.EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
//            {
//                EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555),
//                ReturnAfterSend = false
//            };

            //_socket.Send(buf, 0, buf.Length, true, true);
            return info;
        }
    }
}