using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Game.Rooms;

namespace Shaman.Game.Tests
{
    public class StandaloneServerTests
    {
        [Test]
        public async Task TestLaunchAndReceiveApi()
        {
            var bundleMock = new Mock<IGameBundle>(MockBehavior.Loose);
            var controllerFactoryMock = new Mock<IRoomControllerFactory>(MockBehavior.Loose);
            var controllerMock = new Mock<IRoomController>(MockBehavior.Loose);

            bundleMock.Setup(b => b.GetRoomControllerFactory()).Returns(controllerFactoryMock.Object);
            controllerFactoryMock.Setup(f =>
                f.GetGameModeController(It.IsAny<IRoomContext>(), It.IsAny<ITaskScheduler>(),
                    It.IsAny<IRoomPropertiesContainer>())).Returns(controllerMock.Object);


            var api = StandaloneServerLauncher.Launch(bundleMock.Object, new string[0], "TestGame", "SomeRegion",
                "localhost",
                new List<ushort> {23453}, 7005);
            var serverApi = await api.ApiInitializationTask;

            serverApi.CreateRoom(new Dictionary<byte, object>(), new Dictionary<Guid, Dictionary<byte, object>>());
        }
    }
}