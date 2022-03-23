using System.Net;
using System.Threading.Tasks;
using Bro.BackEnd.Mvc;
using Microsoft.AspNetCore.Mvc;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.MM;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.Routing.Common.Messages;


namespace Shaman.MM.Controllers
{
    public class MatchmakerController : Controller
    {
        private readonly IRoomManager _roomManager;
        private readonly IMatchMakerServerInfoProvider _serverInfoProvider;

        public MatchmakerController(IRoomManager roomManager,
            IMatchMakerServerInfoProvider serverInfoProvider)
        {
            _roomManager = roomManager;
            _serverInfoProvider = serverInfoProvider;
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return new JsonResult(new {Success = true})
            {
                StatusCode = (int) HttpStatusCode.OK
            };
        }

        [HttpPost("updateroomstate")]
        public async Task<ShamanResult> UpdateRoomState(UpdateRoomStateRequest request)
        {
            var response = new UpdateRoomStateResponse();
            _roomManager.UpdateRoomState(request.RoomId, request.CurrentPlayerCount, request.State,
                request.MaxMatchMakingWeight);
            return response;
        }

        [HttpPost("getroominfo")]
        public async Task<ShamanResult> GetRoomInfo(RoomInfoRequest request)
        {
            var response = new RoomInfoResponse();
            var room = _roomManager.GetRoom(request.RoomId);
            if (room == null)
                response.SetError($"Room {request.RoomId} not found");
            else
            {
                response.CreatedDate = room.CreatedOn;
                response.State = room.State;
            }

            return response;
        }

        [HttpPost("actualize")]
        public async Task<ShamanResult> ActualizeGameServer(ActualizeServerOnMatchMakerRequest request)
        {
            var response = new ActualizeServerOnMatchMakerResponse();
            _serverInfoProvider.AddServer(new ServerInfo(request.ServerIdentity, request.Name, request.Region,
                request.HttpPort, request.HttpsPort));
            return response;
        }
    }
}