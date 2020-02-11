using System;
using System.Collections.Concurrent;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Utils.Senders
{
    public class PacketBatchSender : IPacketSender
    {
        private readonly object _sync = new object();
        private readonly ITaskScheduler _taskScheduler;
        private readonly ConcurrentDictionary<IPeerSender, IPacketQueue> _peerToPackets;
        private readonly IPacketSenderConfig _config;
        private readonly ISerializer _serializer;
        private readonly IShamanLogger _logger;
        private PendingTask _sendTaskId;

        public PacketBatchSender(ITaskSchedulerFactory taskSchedulerFactory, IPacketSenderConfig config,
            ISerializer serializer, IShamanLogger logger)
        {
            _config = config;
            _serializer = serializer;
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _peerToPackets = new ConcurrentDictionary<IPeerSender, IPacketQueue>();
        }

        public void AddPacket(MessageBase message, IPeerSender peer)
        {
            var serializedMessage = _serializer.Serialize(message);
            AddPacket(peer, serializedMessage, message.IsReliable, message.IsOrdered);
        }

        public void AddPacket(IPeerSender peer, byte[] data, bool isReliable, bool isOrdered)
        {
            AddPacket(peer, data, 0, data.Length, isReliable, isOrdered);
        }

        public void AddPacket(IPeerSender peer, byte[] data, int offset, int length, bool isReliable, bool isOrdered)
        {
            lock (_sync)
            {
                if (!_peerToPackets.TryGetValue(peer, out var packetsQueue))
                {
                    packetsQueue = new PacketQueue(_config.GetMaxPacketSize());
                    _peerToPackets.TryAdd(peer, packetsQueue);
                }

                packetsQueue.Enqueue(data, offset, length, isReliable, isOrdered);
            }
        }

        public void PeerDisconnected(IPeerSender peer)
        {
            lock (_sync)
            {
                if (_peerToPackets.TryRemove(peer, out var packetQueue))
                {
                    foreach (var packet in packetQueue)
                    {
                        packet.Dispose();
                    }
                }
            }
        }

        public int GetMaxQueueSIze()
        {
            if (!_peerToPackets.Any())
                return 0;

            return _peerToPackets.Max(p => p.Value.Count);
        }

        public int GetAverageQueueSize()
        {
            if (!_peerToPackets.Any())
                return 0;

            return (int) (_peerToPackets.Average(p => p.Value.Count));
        }

        private void Send()
        {
            foreach (var kv in _peerToPackets)
            {
                lock (_sync)
                {
                    while (kv.Value.TryDequeue(out var pack))
                    {
                        kv.Key.Send(pack);
                    }
                }
            }
        }

        public void Start(bool shortLiving)
        {
            if (_sendTaskId != null)
            {
                throw new Exception("PacketSender already started");
            }

            //start send
            _sendTaskId = _taskScheduler.ScheduleOnInterval(Send, 0, _config.GetSendTickTimerMs(), shortLiving);
        }

        public void Stop()
        {
            _taskScheduler.Dispose();
            _sendTaskId = null;

            foreach (var peer in _peerToPackets.Keys)
            {
                if (_peerToPackets.TryRemove(peer, out var q))
                {
                    while (q.TryDequeue(out var pack))
                    {
                        pack.Dispose();
                    }
                }
            }
        }
    }
}