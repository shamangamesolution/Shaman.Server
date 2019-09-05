using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Server.Applications;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.MM.Extensions;
using Shaman.Messages.MM;

//using Shaman.ServerSharedUtilities.Controllers;
//using Shaman.ServerSharedUtilities.Extensions;

namespace Shaman.MM.Controllers
{
    public class MatchmakerController : Controller
    {
        protected ISerializerFactory _serializerFactory;
        protected IApplication Application;
        protected IShamanLogger _logger;
        
        public MatchmakerController(ISerializerFactory serializerFactory, IApplication mmApplication, IShamanLogger logger)
        {
            _serializerFactory = serializerFactory;
            Application = mmApplication;
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

        [HttpPost("actualize")]
        public async Task<ActionResult> Actualize()
        {
            //Request.Body.Position = 0;            
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = MessageBase.DeserializeAs<ActualizeServerRequest>(_serializerFactory, input);
            ActualizeServerResponse response = new ActualizeServerResponse();

            try
            {
                ((MmApplication)Application).ActualizeServer(request);
            }
            catch (Exception ex)
            {
                _logger.Error($"Register server error: {ex}");
                response.ResultCode = ResultCode.RequestProcessingError;
            }
            
            return new FileContentResult(response.Serialize(_serializerFactory), "text/html");

        }
    }
}