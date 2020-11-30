using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.Logging;
using Shaman.Contract.Common;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Tests.Helpers
{
    public static class PackageHelper
    {
        private static readonly ConsoleLogger ConsoleLogger = new ConsoleLogger();

        public static DataPacket GetPacketInfo(MessageBase message)
        {
            var serializerFactory = new BinarySerializer();
            var initMsgArray = serializerFactory.Serialize(message); //message.Serialize(_serializerFactory);


            var packetInfo = new PacketInfo(new DeliveryOptions(false, false), 300, ConsoleLogger,
                new Payload(initMsgArray));

            return new DataPacket(packetInfo.Buffer, 0, packetInfo.Length,
                new DeliveryOptions(message.IsReliable, message.IsOrdered));
        }
    }
}