using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Http;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Client.Tests
{
    class ClientPeerConfig : IShamanClientPeerConfig
    {
        public int PollPackageQueueIntervalMs { get; set; }
        public bool StartOtherThreadMessageProcessing { get; set; }
        public int MaxPacketSize { get; set; }
        public int SendTickMs { get; set; }
    }

    class TestRequestSender:IRequestSender
    {
        private readonly HttpSender _httpSender;

        public TestRequestSender(IShamanLogger logger, ISerializer serializer)
        {
            _httpSender = new HttpSender(logger,serializer);
        }

        public Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request) where T : HttpResponseBase, new()
        {
            return _httpSender.SendRequest<T>(serviceUri, request);
        }

        public Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
        {
            return _httpSender.SendRequest<T>(serviceUri, request, callback);
        }
        
    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            // LocalBundleLauncher.LocalBundleLauncher.Launch();
        }

        [Test]
        public async Task Test1()
        {
            var binarySerializer = new BinarySerializer();
            var logger = new ConsoleLogger();
            var peer = new ShamanClientPeer(logger, new TaskSchedulerFactory(logger), binarySerializer,
                new TestRequestSender(logger, binarySerializer), Mock.Of<IShamanClientPeerListener>(), new ClientPeerConfig
                {
                    PollPackageQueueIntervalMs = 30,
                    MaxPacketSize = 1000,
                    SendTickMs = 30,
                    StartOtherThreadMessageProcessing = true
                });

            await peer.DirectConnectToGameServer("127.0.0.1", 23453, Guid.NewGuid(), Guid.NewGuid(),
                new Dictionary<byte, object>());
        }
    }
}