using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.MM.Servers
{

    
    public class RegisteredServer
    {
        public ServerIdentity Id { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime ActualizedOn { get; set; }
        public int TotalPeers { get; set; }
        public Dictionary<ushort, int> PeersCountPerPort { get; set; }
        public string CreateRoomUrl { get; set; }
        
        private object _syncServer = new object();
        private IRequestSender _requestSender;
        private IShamanLogger _logger;
        
        public ushort GetLessLoadedPort()
        {
            lock (_syncServer)
            {
                if (!PeersCountPerPort.Any())
                {
                    if (Id == null || !Id.Ports.Any())
                        throw new Exception("Port collection is empty");
                    return Id.Ports.First();
                }

                return PeersCountPerPort.OrderBy(p => p.Value).FirstOrDefault().Key;
            }
        }
        
        
        public RegisteredServer(ServerIdentity id, IRequestSender requestSender, string createRoomUrl, IShamanLogger logger)
        {
            RegisteredOn = DateTime.UtcNow;
            Id = id;
            _requestSender = requestSender;
            CreateRoomUrl = createRoomUrl;
            _logger = logger;
            PeersCountPerPort = new Dictionary<ushort, int>();
        }

        public void Actualize(Dictionary<ushort, int> peersCountPerPort)
        {
            PeersCountPerPort = peersCountPerPort;
            TotalPeers = PeersCountPerPort.Sum(p => p.Value);
            ActualizedOn = DateTime.UtcNow;
        }

        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var response = _requestSender.SendRequest<CreateRoomResponse>(CreateRoomUrl, new CreateRoomRequest(properties, players)).Result as CreateRoomResponse;
            
            if (response == null || !response.Success)
            {
                _logger.Error($"CreateRoom error: response is null or error");
                //TODO bad kind of hack
                return Guid.Empty;
            }

            return response.RoomId;
        }

        public bool ActualizedOnNotOlderThan(int timeoutMs)
        {
            return (DateTime.UtcNow - this.ActualizedOn).TotalMilliseconds < timeoutMs;
        }
    }
}