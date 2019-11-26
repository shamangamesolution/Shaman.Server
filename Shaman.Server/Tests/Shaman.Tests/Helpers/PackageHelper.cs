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
        public static DataPacket GetPacketInfo(MessageBase message)
        {
            var serializerFactory = new BinarySerializer();
            var initMsgArray = serializerFactory.Serialize(message);//message.Serialize(_serializerFactory);
            var packetInfo = new PacketInfo(initMsgArray, false, false, 300);
            
            return new DataPacket
            {
                Buffer = packetInfo.Buffer, Offset = 0, Length = packetInfo.Length
            };
        }
    }
}