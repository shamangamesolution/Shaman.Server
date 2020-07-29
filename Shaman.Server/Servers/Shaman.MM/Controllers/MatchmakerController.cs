using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Server.Applications;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.MM.Extensions;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.Serialization;
using Shaman.Serialization.Messages;


namespace Shaman.MM.Controllers
{
    public class MatchmakerController : Controller
    {
        private readonly ISerializer _serializer;
        private readonly IApplication _application;
        private readonly IShamanLogger _logger;
        private readonly IRoomManager _roomManager;
        
        public MatchmakerController(ISerializer serializer, IApplication mmApplication, IShamanLogger logger, IRoomManager roomManager)
        {
            _serializer = serializer;
            _application = mmApplication;
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
        
    }
}