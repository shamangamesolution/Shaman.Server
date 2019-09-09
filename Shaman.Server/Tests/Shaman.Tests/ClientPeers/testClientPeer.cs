using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Client.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests.ClientPeers
{
    public class TestClientPeer 
    {
        private ClientPeer _clientPeer = null;
        private IShamanLogger _logger;
        private object _syncCollection = new object();
        private List<MessageBase> _receivedMessages = new List<MessageBase>();
        private JoinInfo _joinInfo;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        
        public TestClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory)
        {
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _clientPeer = new ClientPeer(logger,_taskSchedulerFactory, 300, 20);
            //_clientPeer.OnPackageReceived += ClientOnPackageReceived;
            
            _taskScheduler.ScheduleOnInterval(() =>
            {
                PacketInfo pack = null;
                while ((pack = _clientPeer.PopNextPacket()) != null)
                {
                    ClientOnPackageReceived(pack);
                }
            }, 0, 20);
        }

        public JoinInfo GetJoinInfo()
        {
            return _joinInfo;
        }
        
        public void Connect(string address, ushort port)
        {
            _clientPeer.Connect(address, port);
        }

        public void Disconnect()
        {
            _clientPeer.Disconnect();
        }

        public List<MessageBase> GetMessageList()
        {
            return _receivedMessages;
        }

        public int GetCountOf(ushort operationCode)
        {
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode);
            return count;
        }
        
        public int GetCountOfSuccessResponses(ushort operationCode)
        {
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode && m.Type == MessageType.Response && ((ResponseBase)m).ResultCode == ResultCode.OK);
            return count;
        }
        public int GetCountOfNotSuccessResponses(ushort operationCode)
        {
            var count = _receivedMessages.Count(m => m.OperationCode == operationCode && m.Type == MessageType.Response && ((ResponseBase)m).ResultCode != ResultCode.OK);
            return count;
        }
        public void Send(MessageBase message)
        {
            _clientPeer.Send(message);
        }

        private void ProcessMessage(byte[] buffer, int offset, int length)
        {
            SerializerFactory bitFac = new SerializerFactory(_logger);
            bitFac.InitializeDefaultSerializers(0, "client");
            //probably bad kind of using
            var message = new ArraySegment<byte>(buffer, offset, length).ToArray();

            var operationCode = MessageBase.GetOperationCode(message);

            _logger.Info($"Message received. Operation code: {operationCode}");
            MessageBase deserialized = MessageFactory.DeserializeMessage(operationCode, bitFac, message);

            _receivedMessages.Add(deserialized);

            //save join info
            if (deserialized is JoinInfoEvent joinInfoEvent)
                _joinInfo = joinInfoEvent.JoinInfo;
        }
        
        private void ClientOnPackageReceived(PacketInfo packet)
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
                packet.RecycleCallback?.Invoke();

            }
        }

        public bool IsConnected()
        {
            return _clientPeer.IsConnected();
        }
    }
}