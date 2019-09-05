using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Server.Senders
{
    public class PacketBatchSender: IPacketSender
    {
        private object _sync = new object();
        private ITaskScheduler _taskScheduler;
        private ConcurrentDictionary<Guid, ConcurrentQueue<PacketInfo>> _peerIdToPackets;
        private ConcurrentDictionary<Guid, IPeer> _peerIdToPeers;
        private IApplicationConfig _config;
        private ISerializerFactory _serializerFactory;
        
        public PacketBatchSender(ITaskSchedulerFactory taskSchedulerFactory, IApplicationConfig config, ISerializerFactory serializerFactory)
        {
            _config = config;
            _serializerFactory = serializerFactory;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _peerIdToPackets = new ConcurrentDictionary<Guid, ConcurrentQueue<PacketInfo>>();
            _peerIdToPeers = new ConcurrentDictionary<Guid, IPeer>();
            //start send
            _taskScheduler.ScheduleOnInterval(Send, 0, config.GetSendTickTimerMs());
        }

        public void AddPacket(MessageBase message, IPeer peer)
        {
            var serializedMessage = message.Serialize(_serializerFactory);
            AddPacket(peer, serializedMessage, message.IsReliable, message.IsOrdered);
        }

        public void AddPacket(IPeer peer, byte[] packet, bool isReliable, bool isOrdered)
        {
            lock (_sync)
            {
                if (!_peerIdToPeers.ContainsKey(peer.GetPeerId()))
                    _peerIdToPeers.TryAdd(peer.GetPeerId(), peer);

                if (!_peerIdToPackets.ContainsKey(peer.GetPeerId()))
                    _peerIdToPackets.TryAdd(peer.GetPeerId(), new ConcurrentQueue<PacketInfo>());

                if (!_peerIdToPackets[peer.GetPeerId()].IsEmpty)
                {
                    var lastElement = _peerIdToPackets[peer.GetPeerId()].Last();
                    if (lastElement != null)
                    {
                        if (lastElement.Length + packet.Length <= _config.GetMaxPacketSize()
                            && lastElement.IsReliable == isReliable
                            && lastElement.IsOrdered == isOrdered)
                        {
                            //add to previous
                            lastElement.Add(packet, isReliable, isOrdered);
                            return;
                        }
                    }
                }

                var packetInfo = new PacketInfo(_config.GetMaxPacketSize());
                packetInfo.Add(packet, isReliable, isOrdered);
                //add new packet
                _peerIdToPackets[peer.GetPeerId()].Enqueue(packetInfo);
            }
        }

        public void PeerDisconnected(Guid peerId)
        {
            _peerIdToPeers.TryRemove(peerId, out var peer);
            _peerIdToPackets.TryRemove(peerId, out var queue);
        }

        public int GetMaxQueueSIze()
        {
            if (!_peerIdToPackets.Any())
                return 0;
            
            return _peerIdToPackets.Max(p => p.Value.Count);

        }

        public int GetAverageQueueSize()
        {
            if (!_peerIdToPackets.Any())
                return 0;
            
            return (int) (_peerIdToPackets.Average(p => p.Value.Count));
        }

        public void Send()
        {
            var toDel = new List<Guid>();
            foreach (var item in _peerIdToPackets)
            {
                if (!_peerIdToPeers.ContainsKey(item.Key))
                {
                    toDel.Add(item.Key);
                    continue;
                }

                lock (_sync)
                {
                    while (!item.Value.IsEmpty)
                    {
                        if (!item.Value.TryDequeue(out var pack))
                            continue;
                        _taskScheduler.ScheduleOnceOnNow(() => _peerIdToPeers[item.Key].Send(pack, pack.IsReliable, pack.IsOrdered));
                    }
                }
            }

            //delete not actual
            foreach (var item in toDel)
                PeerDisconnected(item);
        }
    }
}