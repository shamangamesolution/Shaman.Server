using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Client.Tests
{
    class MyClass : IShamanClientPeerConfig
    {
        public int PollPackageQueueIntervalMs { get; set; }
        public bool StartOtherThreadMessageProcessing { get; set; }
        public int MaxPacketSize { get; set; }
        public int SendTickMs { get; set; }
    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var binarySerializer = new BinarySerializer();
            var logger = new ConsoleLogger();
            var peer = new ShamanClientPeer(logger, new TaskSchedulerFactory(logger), binarySerializer,
                new HttpSender(logger, binarySerializer), Mock.Of<IShamanClientPeerListener>(), new MyClass
                {
                    PollPackageQueueIntervalMs = 30,
                    MaxPacketSize = 1000,
                    SendTickMs = 30,
                    StartOtherThreadMessageProcessing = true
                });

            peer.DirectConnectToGameServer("127.0.0.1", 23453, Guid.NewGuid(), Guid.NewGuid(),
                new Dictionary<byte, object>());
        }
    }
}