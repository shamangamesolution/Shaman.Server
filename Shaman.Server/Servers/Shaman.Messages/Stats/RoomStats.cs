using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;

namespace Shaman.Messages.Stats
{
    
    public class RoomStats
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int PlayersCount { get; set; }
        public Dictionary<ushort, int> MessagesReceivedCount { get; set; }
        public Dictionary<ushort, int> MessagesSentCount { get; set; }
        private List<ushort> _reliableMessages { get; set; }

        public int TotalTrafficReceived { get; set; }
        public int TotalTrafficSent { get; set; }

        public int TotalLiveTimeSec { get; set; }
        
        private object _syncStat = new object();

        public List<int> MaxSendQueueSize = new List<int>() {0};
        public List<int> AverageQueueSize = new List<int>() {0};
        
        public RoomStats(Guid id, int playersCount)
        {
            Id = id;
            PlayersCount = playersCount;
            MessagesReceivedCount = new Dictionary<ushort, int>();
            MessagesSentCount = new Dictionary<ushort, int>();
            _reliableMessages = new List<ushort>();
            TotalTrafficReceived = 0;
            TotalTrafficSent = 0;
            CreatedOn = DateTime.UtcNow;
        }

        private bool IsReliable(ushort operationCode)
        {
            return _reliableMessages.Contains(operationCode);
        }
        
        private void AddReliable(ushort operationCode)
        {
            if (!_reliableMessages.Contains(operationCode))
                _reliableMessages.Add(operationCode);
        }
        
        public void AddSentMessage(ushort operationCode, int sizeInBytes, bool isReliable)
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

        public void AddReceivedMessage(MessageBase message)
        {
            lock (_syncStat)
            {
                if (!MessagesReceivedCount.ContainsKey(message.OperationCode))
                    MessagesReceivedCount.Add(message.OperationCode, 1);
                else
                {
                    MessagesReceivedCount[message.OperationCode]++;
                }
                
                TotalTrafficReceived += message.MessageSizeInBytes;

                if (message.IsReliable)
                    AddReliable(message.OperationCode);
            }
        }
        
        public override string ToString()
        {
            lock (_syncStat)
            {
                TotalLiveTimeSec = (int)((DateTime.UtcNow - CreatedOn).TotalSeconds);
                var info =
                    $"Room {Id}: created on: {CreatedOn}, TotalLiveTime: {TotalLiveTimeSec} sec, players {PlayersCount}, TotalTrafficReceived {TotalTrafficReceived}, TotalTrafficSent {TotalTrafficSent}," +
                    $" Average Max Queue Size: {MaxSendQueueSize.Average()}, Average Average Send Queue Size {AverageQueueSize.Average()}," +
                    $" Received Messages Info: ";

                foreach (var item in MessagesReceivedCount)
                {
                    var isReliableMark = IsReliable(item.Key) ? "*" : "";
                    info += $"{item.Key}{isReliableMark}:{item.Value}, ";
                }

                info += " Sent Messages Count: ";

                foreach (var item in MessagesSentCount)
                {
                    var isReliableMark = IsReliable(item.Key) ? "*" : "";
                    info += $"{item.Key}{isReliableMark}:{item.Value}, ";
                }

                return info;
            }
        }
    }
}