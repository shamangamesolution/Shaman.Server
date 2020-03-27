using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Tests.Helpers
{
    public static class PackageHelper
    {
        public static DataPacket GetPacketInfo(MessageBase message)
        {
            var serializerFactory = new BinarySerializer();
            var initMsgArray = serializerFactory.Serialize(message); //message.Serialize(_serializerFactory);
            var packetInfo = new PacketInfo(initMsgArray, false, false, 300);

            return new DataPacket(packetInfo.Buffer, 0, packetInfo.Length, message.IsReliable);
        }
    }
}