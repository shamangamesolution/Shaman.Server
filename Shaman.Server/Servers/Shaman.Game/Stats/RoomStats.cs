using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Game.Stats
{
    
    public class RoomStats
    {
        private Guid Id { get; }
        private DateTime CreatedOn { get; }
        private int PlayersCount { get; }
        private Dictionary<ushort, int> MessagesReceivedCount { get; }
        private Dictionary<ushort, int> MessagesSentCount { get; }
        private List<ushort> ReliableMessages { get;}

        public int TotalTrafficReceived { get; private set; }
        public int TotalTrafficSent { get; private set; }

        public int TotalLiveTimeSec { get; private set; }
        
        private readonly object _syncStat = new object();

        private readonly List<int> _maxQueueSize = new List<int>() {0};
        private readonly List<int> _averageQueueSize = new List<int>() {0};
        
        public RoomStats(Guid id, int playersCount)
        {
            Id = id;
            PlayersCount = playersCount;
            MessagesReceivedCount = new Dictionary<ushort, int>();
            MessagesSentCount = new Dictionary<ushort, int>();
            ReliableMessages = new List<ushort>();
            TotalTrafficReceived = 0;
            TotalTrafficSent = 0;
            CreatedOn = DateTime.UtcNow;
        }

        public int GetMaxQueueSize()
        {
            lock (_syncStat)
                return _maxQueueSize.Max();
        }

        public double GetAvgQueueSize()
        {
            lock (_syncStat)
                return _averageQueueSize.Average();
        }

        public void AddMaxQueueSize(int maxSize)
        {
            lock (_syncStat)
                _maxQueueSize.Add(maxSize);
        }
        public void AddAvgQueueSize(int avgSize)
        {
            lock (_syncStat)
                _averageQueueSize.Add(avgSize);
        }

        private void AddReliable(ushort operationCode)
        {
            lock (_syncStat)
                if (!ReliableMessages.Contains(operationCode))
                    ReliableMessages.Add(operationCode);
        }
        
        public void TrackSentMessage(int sizeInBytes, bool isReliable, ushort operationCode)
        {
            lock (_syncStat)
            {
                if (!MessagesSentCount.ContainsKey(operationCode))
                    MessagesSentCount.Add(operationCode, 1);
                else
                {
                    MessagesSentCount[operationCode]++;
                }
                
                TotalTrafficSent += sizeInBytes;

                if (isReliable)
                    AddReliable(operationCode);
            }
        }

        public void TrackReceivedMessage(ushort operationCode, int messageLength, bool isReliable)
        {
            lock (_syncStat)
            {
                var messageOperationCode = operationCode;
                if (!MessagesReceivedCount.ContainsKey(messageOperationCode))
                    MessagesReceivedCount.Add(messageOperationCode, 1);
                else
                {
                    MessagesReceivedCount[messageOperationCode]++;
                }

                TotalTrafficReceived += messageLength;

                if (isReliable)
                    AddReliable(messageOperationCode);
            }
        }

        private static ushort GetOperationCode(MessageBase message)
        {
            return message.OperationCode;
        }

        public struct MessageStatistics
        {
            public IList<Tuple<ushort,bool,int>> Sent { get; set; }
            public IList<Tuple<ushort,bool,int>> Received { get; set; }
        }

        public MessageStatistics BuildMessageStatistics()
        {
            var reliableSet = new HashSet<ushort>(ReliableMessages);

            return new MessageStatistics
            {
                Sent = MessagesSentCount
                    .Select(item => new Tuple<ushort, bool, int>(item.Key, reliableSet.Contains(item.Key), item.Value))
                    .OrderBy(t => t.Item1).ToList(),

                Received = MessagesReceivedCount
                    .Select(item => new Tuple<ushort, bool, int>(item.Key, reliableSet.Contains(item.Key), item.Value))
                    .OrderBy(t => t.Item1).ToList(),
            };
        }

        public override string ToString()
        {
            try
            {
                lock (_syncStat)
                {
                    var messageStatistics = BuildMessageStatistics();
                    TotalLiveTimeSec = (int) ((DateTime.UtcNow - CreatedOn).TotalSeconds);
                    return
                        $"Room {Id}: created on: {CreatedOn}, TotalLiveTime: {TotalLiveTimeSec} sec, players {PlayersCount}, TotalTrafficReceived {TotalTrafficReceived}, TotalTrafficSent {TotalTrafficSent}," +
                        $" Max Queue Size: {_maxQueueSize.Max()}, Average Send Queue Size {_averageQueueSize.Average()}," +
                        $" Received Messages Info: {string.Join(", ", messageStatistics.Received.Select(item => $"{item.Item1}{(item.Item2 ? "r" : string.Empty)}:{item.Item3}"))}" +
                        $" Sent Messages Count: {string.Join(", ", messageStatistics.Sent.Select(item => $"{item.Item1}{(item.Item2 ? "r" : string.Empty)}:{item.Item3}"))}";
                }
            }
            catch (Exception e)
            {
                return $"Error building room stats: {e}";
            }
        }
    }
}