using System;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Game.Contract;
using Shaman.Messages;
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

        public async void UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(matchMakerUrl))
                {
                    _logger.Error($"SendRoomStateUpdate error: matchmaker URL is empty in properties container");
                    return;
                }

                await _requestSender.SendRequest<UpdateRoomStateResponse>(matchMakerUrl,
                    new UpdateRoomStateRequest(roomId, roomPlayersCount, roomState), (r) =>
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