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


namespace Shaman.MM.Controllers
{
    public class MatchmakerController : Controller
    {
        private readonly ISerializer _serializer;
        private readonly IApplication _application;
        private readonly IShamanLogger _logger;
        
        public MatchmakerController(ISerializer serializer, IApplication mmApplication, IShamanLogger logger)
        {
            _serializer = serializer;
            _application = mmApplication;
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
        
    }
}