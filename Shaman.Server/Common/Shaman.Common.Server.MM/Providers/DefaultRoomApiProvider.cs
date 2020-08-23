using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Messages.RoomFlow;

namespace Shaman.Common.Server.MM.Providers
{
    public class DefaultRoomApiProvider : IRoomApiProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;

        public DefaultRoomApiProvider(IRequestSender requestSender, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _logger = logger;
        }
        // private string GetUrl(int gameServerId)
        // {
        //     var server = _gameServerList[gameServerId];
        //     if (server == null)
        //         throw new Exception($"GetUrl error: there is no game server with id = {gameServerId}");
        //
        //     return UrlHelper.GetUrl(server.HttpPort, server.HttpsPort, server.Address);
        // }
        
        public async Task<Guid> CreateRoom(string gameServerUrl, Guid roomId, Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var response = await _requestSender.SendRequest<CreateRoomResponse>(gameServerUrl, new CreateRoomRequest(properties, players)
            {
                RoomId = roomId
            });
            
            if (!response.Success)
            {
                var msg = $"CreateRoom error: {response.Message}";
                _logger.Error(msg);
                throw new Exception(msg);
            }
            
            return response.RoomId;
        }
        
        public async Task UpdateRoom(string gameServerUrl, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId)
        {
            var response = await _requestSender.SendRequest<UpdateRoomResponse>(gameServerUrl, new UpdateRoomRequest(roomId, players));
            
            if (!response.Success)
            {
                var msg = $"UpdateRoom error: {response.Message}";
                _logger.Error(msg);
                throw new Exception(msg);
            }
        }
    }
}