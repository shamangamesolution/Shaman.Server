using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.RoomFlow;

namespace Client
{
    public class ServerClient
    {
        private readonly HttpSender _httpSender;
        private readonly string _serverApiEndpoint;

        public ServerClient()
        {
            _httpSender = new HttpSender(new ConsoleLogger(), new BinarySerializer());
            _serverApiEndpoint = "http://localhost:7005";
        }

        public async Task<Guid> CreateRoom(params Guid[] playerIds)
        {
            var dictionary = playerIds.ToDictionary(k => k, v => new Dictionary<byte, object>());

            var createRoomRequest = new CreateRoomRequest(new Dictionary<byte, object>(), dictionary);
            var res = await _httpSender.SendRequest<CreateRoomResponse>(_serverApiEndpoint,
                createRoomRequest);
            if (!res.Success)
            {
                throw new Exception($"Error creating room: {res.Message}");
            }

            return res.RoomId;
        }
    }
}