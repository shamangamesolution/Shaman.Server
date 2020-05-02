using System;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;

namespace Sample.Game.GamePlay.Controllers
{
    public class TestModeController : GameModeControllerBase
    {
        public TestModeController(IRoomContext room, IShamanLogger logger, IRequestSender requestSender, ITaskScheduler taskScheduler, IStorageContainer storageContainer, IRoomPropertiesContainer roomPropertiesContainer, IBackendProvider backendProvider, ISerializer serializerFactory) : base(room, logger, requestSender, taskScheduler, storageContainer, roomPropertiesContainer, backendProvider, serializerFactory)
        {
        }

        public override void ProcessNewPlayer(Player player, Guid sessionId, int backendId)
        {
        }

        public override bool ProcessCharacterSpawn(int playerIndex)
        {
            throw new NotImplementedException();
        }

        public override void ProcessDeadCharacter(int playerIndex)
        {
            throw new NotImplementedException();
        }
    }
}