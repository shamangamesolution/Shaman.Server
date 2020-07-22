using System.Collections;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Contract;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Utils.Tests
{
    public class PacketQueueTests
    {
        private static readonly ConsoleLogger ConsoleLogger = new ConsoleLogger();

        [Test]
        public void TestPackagingOneTypeMessages()
        {
            var packetQueue = new PacketQueue(100, ConsoleLogger);

            packetQueue.Count.Should().Be(0);

            packetQueue.Enqueue(new DeliveryOptions(false, false), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new DeliveryOptions(false, false), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new DeliveryOptions(false, false), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new DeliveryOptions(true, false), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(2);
            packetQueue.Enqueue(new DeliveryOptions(true, false), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(2);
            packetQueue.Enqueue(new DeliveryOptions(true, true), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(3);
            packetQueue.Enqueue(new DeliveryOptions(true, true), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(3);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(4);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {1}, 0, 1));
            packetQueue.Count.Should().Be(4);

            foreach (var packet in packetQueue)
            {
                packet.Length.Should().BeGreaterThan(1);
            }

            foreach (var packet in (IEnumerable) packetQueue)
            {
                packet.Should().BeOfType<PacketInfo>();
            }
        }

        [Test]
        public void TestPackagingIntoMaxSize()
        {
            var packetQueue = new PacketQueue(10, ConsoleLogger);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1}, 0, 2));
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {2, 1}, 0, 2));
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {2, 1}, 0, 2));
            packetQueue.Count.Should().Be(2);
        }

        [Test]
        public void TestPackagingIntoMaxSizeWithLength()
        {
            var packetQueue = new PacketQueue(10, ConsoleLogger);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1, 4, 5, 6}, 0, 2));
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {2, 1, 4, 5, 6}, 0, 2));
            packetQueue.Count.Should().Be(1);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {2, 1}, 0, 2));
            packetQueue.Count.Should().Be(2);
        }

        [Test]
        public void TestPackagingSize()
        {
            var packetQueue = new PacketQueue(10, ConsoleLogger);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1}, 0, 2));
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {2, 1}, 0, 2));
            packetQueue.Count.Should().Be(1);
            packetQueue.TryDequeue(out var packet).Should().BeTrue();
            packet.Length.Should().Be(4 + 5); //with metainfo
        }

        [Test]
        public void TestPackagingSizeWithLength()
        {
            var packetQueue = new PacketQueue(10, ConsoleLogger);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1, 4, 5, 6}, 1, 2));
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {2, 1, 4, 5, 6}, 1, 2));
            packetQueue.Count.Should().Be(1);
            packetQueue.TryDequeue(out var packet).Should().BeTrue();
            packet.Length.Should().Be(4 + 5); //with metainfo
        }

        [Test]
        public void TestTwoPayloads()
        {
            var packetQueue = new PacketQueue(10, ConsoleLogger);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1, 4, 5, 6}, 1, 2),
                new Payload(new byte[] {2, 1, 4, 5, 6}, 1, 2));
            packetQueue.Count.Should().Be(1);
            packetQueue.TryDequeue(out var packet).Should().BeTrue();
            packet.Length.Should().Be(4 + 3); //with metainfo
        }
        [Test]
        public void TestTwoPayloads2()
        {
            var packetQueue = new PacketQueue(13, ConsoleLogger);

            packetQueue.Count.Should().Be(0);
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1, 4, 5, 6}, 1, 2),
                new Payload(new byte[] {2, 1, 4, 5, 6}, 1, 2));
            packetQueue.Enqueue(new DeliveryOptions(false, true), new Payload(new byte[] {3, 1, 4, 5, 6}, 1, 2),
                new Payload(new byte[] {2, 1, 4, 5, 6}, 1, 2));
            packetQueue.Count.Should().Be(1);
            packetQueue.TryDequeue(out var packet).Should().BeTrue();
            packet.Length.Should().Be(8 + 5); //with metainfo
        }
    }
}