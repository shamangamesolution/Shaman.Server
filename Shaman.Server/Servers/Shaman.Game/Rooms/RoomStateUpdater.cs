using System;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Messages.MM;

namespace Shaman.Game.Rooms
{
    public class RoomStateUpdater : IRoomStateUpdater
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;

        public RoomStateUpdater(IRequestSender requestSender, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _logger = logger;
        }

        public async Task UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl, int maxMatchMakingWeight)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(matchMakerUrl))
                {
                    _logger.Error($"SendRoomStateUpdate error: matchmaker URL is empty in properties container");
                    return;
                }

                await _requestSender.SendRequest<UpdateRoomStateResponse>(matchMakerUrl,
                    new UpdateRoomStateRequest(roomId, roomPlayersCount, roomState, maxMatchMakingWeight), (r) =>
                    {
                        if (!r.Success)
                        {
                            _logger.Error($"Room update error: {r.Message}");
                        }
                        else
                        {
                            _logger.Debug($"Room update to {matchMakerUrl} with players count {roomPlayersCount}, state {roomState} successful");
                        }
                    });
            }
            catch (Exception e)
            {
                _logger.Error($"Update room state error: {e}");
            }
        }
    }
}