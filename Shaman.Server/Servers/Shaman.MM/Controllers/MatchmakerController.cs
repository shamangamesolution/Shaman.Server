using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Utils.Logging;
using Shaman.Contract.Common.Logging;
using Shaman.MM.Extensions;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.Serialization;
using Shaman.Serialization.Messages;


namespace Shaman.MM.Controllers
{
    public class MatchmakerController : Controller
    {
        private readonly ISerializer _serializer;
        private readonly IShamanLogger _logger;
        private readonly IRoomManager _roomManager;
        
        public MatchmakerController(ISerializer serializer, IShamanLogger logger, IRoomManager roomManager)
        {
            _serializer = serializer;
            _logger = logger;
            _roomManager = roomManager;
        }
        
        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return new JsonResult(new { Success = true })
            {
                StatusCode = (int) HttpStatusCode.OK                
            };
        }
        
        [HttpPost("updateroomstate")]
        public async Task<ActionResult> UpdateRoomState()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = _serializer.DeserializeAs<UpdateRoomStateRequest>(input);
            var response = new UpdateRoomStateResponse();

            try
            {
                _roomManager.UpdateRoomState(request.RoomId, request.CurrentPlayerCount, request.State);
            }
            catch (Exception ex)
            {
                _logger.Error($"Update room state error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(_serializer.Serialize(response), "text/html");
        }
        
        [HttpPost("getroominfo")]
        public async Task<ActionResult> GetRoomInfo()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = _serializer.DeserializeAs<RoomInfoRequest>(input);
            var response = new RoomInfoResponse();

            try
            {
                var room = _roomManager.GetRoom(request.RoomId);
                if (room == null)
                    response.SetError($"Room {request.RoomId} not found");
                else
                {
                    response.CreatedDate = room.CreatedOn;
                    response.IsOpen = room.IsOpen();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Update room state error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(_serializer.Serialize(response), "text/html");
        }
        
    }
}