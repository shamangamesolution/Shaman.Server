using System.Collections;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Utils.Tests
{
    public class PacketQueueTests
    {
        [Test]
        public void TestPackagingOneTypeMessages()
        {
            var packetQueue = new PacketQueue(100);

            packetQueue.Count.Should().Be(0);

            packetQueue.Enqueue(new byte[] {1}, false, false);
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new byte[] {1}, false, false);
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, false, false);
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, true, false);
            packetQueue.Count.Should().Be(2);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, true, false);
            packetQueue.Count.Should().Be(2);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, true, true);
            packetQueue.Count.Should().Be(3);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, true, true);
            packetQueue.Count.Should().Be(3);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, false, true);
            packetQueue.Count.Should().Be(4);
            packetQueue.Enqueue(new byte[] {1}, 0, 1, false, true);
            packetQueue.Count.Should().Be(4);

            foreach (var packet in packetQueue)
            {
                packet.Length.Should().BeGreaterThan(1);
            }
            
            foreach (var packet in (IEnumerable)packetQueue)
            {
                packet.Should().BeOfType<PacketInfo>();
            }
        }

        [Test]
        public void TestPackagingIntoMaxSize()
        {
            var packetQueue = new PacketQueue(10);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new byte[] {3, 1}, 0, 2, false, true);
            packetQueue.Enqueue(new byte[] {2, 1}, 0, 2, false, true);
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new byte[] {2, 1}, 0, 2, false, true);
            packetQueue.Count.Should().Be(2);
        }

        [Test]
        public void TestPackagingIntoMaxSizeWithLength()
        {
            var packetQueue = new PacketQueue(10);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new byte[] {3, 1, 4, 5, 6}, 0, 2, false, true);
            packetQueue.Enqueue(new byte[] {2, 1, 4, 5, 6}, 0, 2, false, true);
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new byte[] {2, 1}, 0, 2, false, true);
            packetQueue.Count.Should().Be(2);
        }

        [Test]
        public void TestPackagingSize()
        {
            var packetQueue = new PacketQueue(10);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new byte[] {3, 1}, false, true);
            packetQueue.Enqueue(new byte[] {2, 1}, false, true);
            packetQueue.Count.Should().Be(1);
            packetQueue.TryDequeue(out var packet).Should().BeTrue();
            packet.Length.Should().Be(4 + 5);//with metainfo
        }

        [Test]
        public void TestPackagingSizeWithLength()
        {
            var packetQueue = new PacketQueue(10);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new byte[] {3, 1, 4, 5, 6}, 1, 2, false, true);
            packetQueue.Enqueue(new byte[] {2, 1, 4, 5, 6}, 1, 2, false, true);
            packetQueue.Count.Should().Be(1);
            packetQueue.TryDequeue(out var packet).Should().BeTrue();
            packet.Length.Should().Be(4 + 5);//with metainfo
        }
    }
}