using System;
using System.Collections.Concurrent;
using System.Linq;
using Shaman.Common.Udp.Peers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Udp.Senders
{
    
    /// <summary>
    /// todo explore on packet design
    /// may be it would be better to packet also by broadcast type
    /// to reduce memory consumption for SendMany-cases (in cost of package reducing) 
    /// </summary>
    public class PacketBatchSender : IPacketSender
    {
        private readonly object _sync = new object();
        private readonly ITaskScheduler _taskScheduler;
        private readonly ConcurrentDictionary<IPeerSender, IPacketQueue> _peerToPackets;
        private readonly IPacketSenderConfig _config;
        private readonly IShamanLogger _logger;
        private IPendingTask _sendTaskId;

        public PacketBatchSender(ITaskSchedulerFactory taskSchedulerFactory, IPacketSenderConfig config,
            IShamanLogger logger)
        {
            _config = config;
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _peerToPackets = new ConcurrentDictionary<IPeerSender, IPacketQueue>();
        }

        public void AddPacket(IPeerSender peer, DeliveryOptions deliveryOptions, Payload payload)
        {
            lock (_sync)
            {
                var packetsQueue = GetPeerQueue(peer);
                packetsQueue.Enqueue(deliveryOptions, payload);
            }
        }
        public void AddPacket(IPeerSender peer, DeliveryOptions deliveryOptions, Payload payload1, Payload payload2)
        {
            lock (_sync)
            {
                var packetsQueue = GetPeerQueue(peer);
                packetsQueue.Enqueue(deliveryOptions, payload1, payload2);
            }
        }

        private IPacketQueue GetPeerQueue(IPeerSender peer)
        {
            if (!_peerToPackets.TryGetValue(peer, out var packetsQueue))
            {
                packetsQueue = new PacketQueue(_config.MaxPacketSize, _logger);
                _peerToPackets.TryAdd(peer, packetsQueue);
            }
            return packetsQueue;
        }

        public void CleanupPeerData(IPeerSender peer)
        {
            if (_peerToPackets.TryRemove(peer, out var packetQueue))
            {
                lock (_sync)
                {
                    foreach (var packet in packetQueue)
                    {
                        packet.Dispose();
                    }
                    packetQueue.Clear();
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

            return (int) _peerToPackets.Average(p => p.Value.Count);
        }
        public int GetKnownPeersCount()
        {
            return _peerToPackets.Count;
        }

        private void Send()
        {
            foreach (var kv in _peerToPackets)
            {
                lock (_sync)
                {
                    while (kv.Value.TryDequeue(out var pack))
                    {
                        using (pack)
                        {
                            kv.Key.Send(pack);
                        }
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
            _sendTaskId = _taskScheduler.ScheduleOnInterval(Send, 0, _config.SendTickTimeMs, shortLiving);
        }

        public void Stop()
        {
            _taskScheduler.Dispose();
            _sendTaskId = null;

            foreach (var peer in _peerToPackets.Keys)
            {
                if (_peerToPackets.TryRemove(peer, out var q))
                {
                    lock (_sync)
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
}