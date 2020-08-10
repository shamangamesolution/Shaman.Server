using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Utils.Tests
{
    public class PacketInfoTests
    {
        private static readonly ConsoleLogger ConsoleLogger = new ConsoleLogger();

        [Test]
        public void Test()
        {
            var packetInfo = new PacketInfo(new DeliveryOptions(false, true), 100, ConsoleLogger, new Payload(new byte[] {1, 2, 3}, 0, 3));

            packetInfo.Append(new Payload(new byte[] {2, 1}));
            packetInfo.Append(new Payload(new byte[] {3, 2, 3, 1}, 0, 4));

            var offsets = PacketInfo.GetOffsetInfo(packetInfo.Buffer, packetInfo.Offset).ToArray();
            offsets.Length.Should().Be(3);

            offsets[0].Length.Should().Be(3);
            offsets[1].Length.Should().Be(2);
            offsets[2].Length.Should().Be(4);

            new ArraySegment<byte>(packetInfo.Buffer, offsets[0].Offset, offsets[0].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {1, 2, 3});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[1].Offset, offsets[1].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {2, 1});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[2].Offset, offsets[2].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {3, 2, 3, 1});
        }

        [Test]
        public void OffsetTest()
        {
            var packetInfo = new PacketInfo(new DeliveryOptions(false, true), 100, ConsoleLogger, new Payload(new byte[] {0, 0, 1, 2, 3, 9, 9}, 2, 3));

            packetInfo.Append(new Payload(new byte[] {2, 1}));
            packetInfo.Append(new Payload(new byte[] {3, 1}, 0, 2));
            packetInfo.Append(new Payload(new byte[] {4, 2, 3, 1, 8}, 0, 4));
            packetInfo.Append(new Payload(new byte[] {8, 5, 2, 3, 1}, 1, 4));
            packetInfo.Append(new Payload(new byte[] {8, 6, 2, 3, 1, 0}, 1, 4));

            var offsets = PacketInfo.GetOffsetInfo(packetInfo.Buffer, packetInfo.Offset).ToArray();
            offsets.Length.Should().Be(6);

            offsets[0].Length.Should().Be(3);
            offsets[1].Length.Should().Be(2);
            offsets[2].Length.Should().Be(2);
            offsets[3].Length.Should().Be(4);
            offsets[4].Length.Should().Be(4);
            offsets[5].Length.Should().Be(4);

            new ArraySegment<byte>(packetInfo.Buffer, offsets[0].Offset, offsets[0].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {1, 2, 3});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[1].Offset, offsets[1].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {2, 1});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[2].Offset, offsets[2].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {3, 1});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[3].Offset, offsets[3].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {4, 2, 3, 1});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[4].Offset, offsets[4].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {5, 2, 3, 1});
            new ArraySegment<byte>(packetInfo.Buffer, offsets[5].Offset, offsets[5].Length).ToArray().Should()
                .BeEquivalentTo(new byte[] {6, 2, 3, 1});
        }

        [Test]
        public void TestDoubleDispose()
        {
            var mock = new Mock<IShamanLogger>();
            
            var packetInfo = new PacketInfo(new DeliveryOptions(false, true), 100, mock.Object, new Payload(new byte[] {0, 0, 1, 2, 3, 9, 9}, 2, 3));

            packetInfo.Dispose();
            mock.Verify(s => s.Error(It.IsAny<string>()), Times.Never);
            packetInfo.Dispose();

            mock.Verify(s => s.Error("DOUBLE_RENT_RETURN in PacketInfo"), Times.Once);
        }
    }
}