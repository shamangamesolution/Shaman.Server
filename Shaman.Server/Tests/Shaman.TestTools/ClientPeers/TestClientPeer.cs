using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Client.Providers;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.LiteNetLibAdapter;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.TestTools.ClientPeers
{
    public class TestClientPeer: IDisposable
    {
        private class RawMessage
        {
            public ushort OperationCode { get; set; }
            public byte[] Data { get; set; }
            public int Offset { get; set; }
            public int Length { get; set; }
        }

        private readonly ClientPeer _clientPeer;
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;
        private readonly object _syncCollection = new object();
        private List<RawMessage> _receivedMessages = new List<RawMessage>();
        private JoinInfo _joinInfo;
        private readonly ITaskScheduler _taskScheduler;

        public Guid SessionId { get; set; }

        private readonly List<Route> _routeTable = new List<Route>();

        public string MmAddress => _routeTable.First().MatchMakerAddress;
        public ushort MmPort => _routeTable.First().MatchMakerPort;
        public string BackendUrl => $"{_routeTable.First().BackendProtocol}://{_routeTable.First().BackendAddress}:{_routeTable.First().BackendPort}/";
        public int BackendId => _routeTable.First().BackendId;
        public Action<IDisconnectInfo> OnDisconnectedFromServer;

        public TestClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _clientPeer = new ClientPeer(logger, new LiteNetClientTransportLayerFactory(),taskSchedulerFactory, 300, 10);
            //_clientPeer.OnPackageReceived += ClientOnPackageReceived;

            _clientPeer.OnDisconnectedFromServer = OnDisconnected;

            _taskScheduler.ScheduleOnInterval(() =>
            {
                IPacketInfo pack = null;
                while ((pack = _clientPeer.PopNextPacket()) != null)
                {
                    ClientOnPackageReceived(pack);
                }
            }, 0, 10);
        }

        private void OnDisconnected(IDisconnectInfo disconnectInfo)
        {
            _logger.Info($"Disconnected from server: {disconnectInfo.Reason}");
            OnDisconnectedFromServer?.Invoke(disconnectInfo);
        }


        public async Task LoadRoutes(string routerUrl, string clientVersion)
        {
            var httpSender = new TestClientHttpSender(_logger, new BinarySerializer());
            var routerClient = new TestRouterClient(httpSender, _logger, routerUrl);
            var clientServerInfoProvider =
                new ClientServerInfoProvider(_logger, routerClient);
            _routeTable.AddRange(await clientServerInfoProvider.GetRoutes(routerUrl, clientVersion));
            _routeTable.Should().NotBeEmpty();
        }

        public JoinInfo GetJoinInfo()
        {
            return _joinInfo;
        }

        public async Task Connect(string address, ushort port)
        {
            _clientPeer.Connect(address, port);
            await WaitFor<ConnectedEvent>();
        }

        public void Disconnect()
        {
            _clientPeer.Disconnect();
        }

        public void ClearMessages()
        {
            _receivedMessages = new List<RawMessage>();
        }
        public int CountOf<T>() where T : MessageBase, new()
        {
            var operationCode = (new T()).OperationCode;
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode);
            return count;
        }
        public void Send(MessageBase message)
        {
            _clientPeer.Send(message, message.IsReliable, message.IsOrdered);
        }
        public void SendEvent<TMessage>(TMessage eve) where TMessage : MessageBase
        {
            _clientPeer.Send(new BundleMessageWrapper<TMessage>(eve), eve.IsReliable, eve.IsOrdered);
        }
        public async Task<TResponse> Send<TResponse>(RequestBase message) where TResponse:ResponseBase, new()
        {
            _clientPeer.Send(message, message.IsReliable, message.IsOrdered);
            var responseBase = await WaitFor<TResponse>(resp => resp.Success);
            ClearMessages();
            return responseBase;
        }

        private void ProcessMessage(byte[] buffer, int offset, int length)
        {
            var operationCode = MessageBase.GetOperationCode(buffer, offset);
            _logger.LogInfo($"Message received. Operation code: {operationCode}");
            if (operationCode != ShamanOperationCode.Bundle)
            {
                _receivedMessages.Add(new RawMessage
                {
                    Data = buffer.ToArray(),
                    Offset = offset,
                    Length = length,
                    OperationCode = operationCode
                });
            }
            else
            {
                var bundleOperationCode = MessageBase.GetOperationCode(buffer, offset + 1);
                _logger.LogInfo($"It is bundle message ! {bundleOperationCode}");
                _receivedMessages.Add(new RawMessage
                {
                    Data = buffer.ToArray(),
                    Offset = offset + 1,
                    Length = length - 1,
                    OperationCode = bundleOperationCode
                });
            }


            //save join info
            if (operationCode == ShamanOperationCode.JoinInfo)
                _joinInfo = _serializer.DeserializeAs<JoinInfoEvent>(buffer, offset, length).JoinInfo;
        }

        private void ClientOnPackageReceived(IPacketInfo packet)
        {
            lock (_syncCollection)
            {
                var offsets = PacketInfo.GetOffsetInfo(packet.Buffer, packet.Offset);
                foreach (var item in offsets)
                {
                    try
                    {
                        ProcessMessage(packet.Buffer, item.Offset, item.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error processing message: {ex}");
                    }
                }
                packet.Dispose();

            }
        }

        public bool IsConnected()
        {
            return _clientPeer.IsConnected();
        }

        public async Task<T> WaitFor<T>(Func<T, bool> condition = null,
            int timeoutMs = 5000, int checkPeriod = 100) where T : MessageBase, new()
        {
            var operationCode = (new T()).OperationCode;
            var stopwatch = Stopwatch.StartNew();
            RawMessage msg = null;
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                msg = _receivedMessages.LastOrDefault(m =>
                    m.OperationCode == operationCode);

                if (msg != null)
                {
                    var deserialized = _serializer.DeserializeAs<T>(msg.Data, msg.Offset, msg.Length);
                    if (condition == null || condition.Invoke(deserialized))
                    {
                        _logger.LogInfo($"Condition for event {typeof(T)} was matched for {stopwatch.ElapsedMilliseconds}ms");
                        return deserialized;
                    }

                }

                await Task.Delay(checkPeriod);
            }

            throw new TestClientPeerException(this,
                msg == null
                    ? $"Event of type {typeof(T)} was not received for {stopwatch.ElapsedMilliseconds}ms."
                    : $"Event of type {typeof(T)} with specified condition was not received for {stopwatch.ElapsedMilliseconds}ms.");
        }

        public async Task<TimeSpan> Ping()
        {
            ClearMessages();
            var stopwatch = Stopwatch.StartNew();
            var request = new PingRequest();
            Send(request);
            await WaitFor((PingResponse response) => response.ResultCode == ResultCode.OK && response.SourceTicks == request.SourceTicks, 2000);
            return stopwatch.Elapsed;
        }

        public void Dispose()
        {
            _taskScheduler.Dispose();
        }
    }
}