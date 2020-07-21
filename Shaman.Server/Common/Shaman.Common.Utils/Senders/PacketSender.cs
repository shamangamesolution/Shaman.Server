using System;
using System.Collections.Concurrent;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Utils.Senders
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
        private PendingTask _sendTaskId;

        public PacketBatchSender(ITaskSchedulerFactory taskSchedulerFactory, IPacketSenderConfig config,
            IShamanLogger logger)
        {
            _config = config;
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _peerToPackets = new ConcurrentDictionary<IPeerSender, IPacketQueue>();
        }

        public void AddPacket(IPeerSender peer, byte[] data, int offset, int length, bool isReliable,
            bool isOrdered)
        {
            lock (_sync)
            {
                if (!_peerToPackets.TryGetValue(peer, out var packetsQueue))
                {
                    packetsQueue = new PacketQueue(_config.GetMaxPacketSize(), _logger);
                    _peerToPackets.TryAdd(peer, packetsQueue);
                }

                packetsQueue.Enqueue(data, offset, length, isReliable, isOrdered);
            }
        }

        public void PeerDisconnected(IPeerSender peer)
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