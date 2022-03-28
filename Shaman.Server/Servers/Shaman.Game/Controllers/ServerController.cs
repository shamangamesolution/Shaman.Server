using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Mvc;
using Shaman.Contract.Bundle;
using Shaman.Messages.RoomFlow;

namespace Shaman.Game.Controllers
{
    public class ServerController : Controller
    {
        private readonly IGameServerApi _gameServerApi;

        public ServerController(IGameServerApi gameServerApi)
        {
            _gameServerApi = gameServerApi;
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return new JsonResult(new {Success = true})
            {
                StatusCode = (int) HttpStatusCode.OK
            };
        }

        [HttpPost("createroom")]
        public async Task<ShamanResult> CreateRoom(CreateRoomRequest request)
        {
            return new CreateRoomResponse
            {
                RoomId = _gameServerApi.CreateRoom(request.Properties, request.Players, request.RoomId)
            };
        }

        [HttpPost("canjoinroom")]
        public async Task<ShamanResult> CanJoinRoom(CanJoinRoomRequest request)
        {
            return new CanJoinRoomResponse
            {
                CanJoin = _gameServerApi.CanJoinRoom(request.RoomId)
            };
        }

        [HttpPost("updateroom")]
        public async Task<ShamanResult> UpdateRoom(UpdateRoomRequest request)
        {
            var response = new UpdateRoomResponse();
            _gameServerApi.UpdateRoom(request.RoomId, request.Players);
            return response;
        }
    }
}