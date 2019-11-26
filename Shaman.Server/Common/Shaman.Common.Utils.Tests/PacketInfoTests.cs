using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Utils.Tests
{
    public class PacketInfoTests
    {
        [Test]
        public void Test()
        {
            var packetInfo = new PacketInfo(new byte[] {1, 2, 3}, false, true, 100);

            packetInfo.Append(new byte[] {2, 1});
            packetInfo.Append(new byte[] {3, 2, 3, 1});

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
    }
}