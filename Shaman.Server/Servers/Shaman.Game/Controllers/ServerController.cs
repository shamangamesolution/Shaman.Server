using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Server.Applications;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Game.Extensions;
using Shaman.Messages.RoomFlow;

namespace Shaman.Game.Controllers
{
    public class ServerController : Controller
    {
        protected readonly ISerializer Serializer;
        protected readonly IApplication Application;
        protected readonly IShamanLogger _logger;

        public ServerController(ISerializer serializer, IApplication gameApplication, IShamanLogger logger)
        {
            Serializer = serializer;
            Application = gameApplication;
            _logger = logger;
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

            var request = Serializer.DeserializeAs<CreateRoomRequest>(input);
            CreateRoomResponse response = new CreateRoomResponse();

            try
            {
                var roomId = ((GameApplication) Application).CreateRoom(request.Properties, request.Players);
                response.RoomId = roomId;
            }
            catch (Exception ex)
            {
                _logger.Error($"Create room error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(Serializer.Serialize(response), "text/html");

        }
        
        [HttpPost("updateroom")]
        public async Task<ActionResult> UpdateRoom()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = Serializer.DeserializeAs<UpdateRoomRequest>(input);
            var response = new UpdateRoomResponse();

            try
            {
                ((GameApplication) Application).UpdateRoom(request.RoomId, request.Players);
            }
            catch (Exception ex)
            {
                _logger.Error($"Update room error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(Serializer.Serialize(response), "text/html");

        }
    }
}