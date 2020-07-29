using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Game.Api;
using Shaman.Game.Extensions;
using Shaman.Messages.RoomFlow;

namespace Shaman.Game.Controllers
{
    public class ServerController : Controller
    {
        private readonly ISerializer _serializer;
        private readonly IShamanLogger _logger;
        private readonly IGameServerApi _gameServerApi;

        public ServerController(ISerializer serializer, IShamanLogger logger, IGameServerApi gameServerApi)
        {
            _serializer = serializer;
            _logger = logger;
            _gameServerApi = gameServerApi;
        }
        
        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return new JsonResult(new { Success = true })
            {
                StatusCode = (int) HttpStatusCode.OK                
            };
        }
        
        [HttpPost("createroom")]
        public async Task<ActionResult> CreateRoom()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = _serializer.DeserializeAs<CreateRoomRequest>(input);
            CreateRoomResponse response = new CreateRoomResponse();

            try
            {
                response.RoomId = _gameServerApi.CreateRoom(request.Properties, request.Players, request.RoomId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Create room error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(_serializer.Serialize(response), "text/html");

        }
        
        [HttpPost("canjoinroom")]
        public async Task<ActionResult> CanJoinRoom()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = _serializer.DeserializeAs<CanJoinRoomRequest>(input);
            var response = new CanJoinRoomResponse();

            try
            {
                var roomInfo = _gameServerApi.GetRoomInfo(request.RoomId);
                response.CanJoin = roomInfo != null && roomInfo.IsOpen;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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
                var roomInfo = _gameServerApi.GetRoomInfo(request.RoomId);
                if (roomInfo == null)
                {
                    response.SetError($"Room not {request.RoomId} found");
                }
                else
                {
                    response.CreatedDate = roomInfo.CreatedDate;
                    response.IsOpen = roomInfo.IsOpen;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(_serializer.Serialize(response), "text/html");

        }
        
        [HttpPost("updateroom")]
        public async Task<ActionResult> UpdateRoom()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = _serializer.DeserializeAs<UpdateRoomRequest>(input);
            var response = new UpdateRoomResponse();

            try
            {
                _gameServerApi.UpdateRoom(request.RoomId, request.Players);
            }
            catch (Exception ex)
            {
                _logger.Error($"Update room error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(_serializer.Serialize(response), "text/html");

        }
    }
}