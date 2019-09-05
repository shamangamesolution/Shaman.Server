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
        protected ISerializerFactory _serializerFactory;
        protected IApplication Application;
        protected IShamanLogger _logger;

        public ServerController(ISerializerFactory serializerFactory, IApplication gameApplication, IShamanLogger logger)
        {
            _serializerFactory = serializerFactory;
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

            var request = MessageBase.DeserializeAs<CreateRoomRequest>(_serializerFactory, input);
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
            
            return new FileContentResult(response.Serialize(_serializerFactory), "text/html");

        }
    }
}