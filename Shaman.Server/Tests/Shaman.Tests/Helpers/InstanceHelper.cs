using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle.Stats;
using Shaman.Contract.Common.Logging;
using Shaman.Game;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.LiteNetLibAdapter;
using Shaman.Messages;
using Shaman.Serialization;
using Shaman.Tests.Configuration;
using Shaman.Tests.GameModeControllers;

namespace Shaman.Tests.Helpers
{
    public class InstanceHelper
    {
        private static Guid CreateRoomDelegate(Dictionary<byte, object> properties, GameApplication gameApplication, Guid roomId)
        {
            return gameApplication.CreateRoom(properties, new Dictionary<Guid, Dictionary<byte, object>>(), roomId);
        }

        private static void UpdateRoomDelegate(Guid roomId, GameApplication gameApplication)
        {
            gameApplication?.UpdateRoom(roomId, new Dictionary<Guid, Dictionary<byte, object>>());
        }

        private static IShamanMessageSender GetSHamanMessageSender(ISerializer serializer, IPacketSender packetSender, IPacketSenderConfig config, IShamanLogger logger)
        {
            return new ShamanMessageSender(new ShamanSender(serializer, packetSender, config));
        }
        
        public static GameApplication GetGame(ushort gamePort, bool isAuthOn = false)
        {
            return GetGame(new List<ushort> {gamePort}, isAuthOn);
        }
        
        public static GameApplication GetGame(List<ushort> gamePorts, bool isAuthOn = false)
        {
            var _roomControllerFactory = new FakeRoomControllerFactory();
            var serverLogger = new ConsoleLogger("G ", LogLevel.Error | LogLevel.Info | LogLevel.Debug);
            var socketFactory = new LiteNetSockFactory(serverLogger);
            var serializer = new BinarySerializer();
            var taskSchedulerFactory = new TaskSchedulerFactory(serverLogger);
            var protectionManagerConfig = new ConnectionDdosProtectionConfig(300, 5000, 5000, 60000);
            var connectionDdosProtection = new ConnectDdosProtection(protectionManagerConfig,taskSchedulerFactory, serverLogger, new GameMetricsStub());
            var protectionManager = new ProtectionManager(connectionDdosProtection, protectionManagerConfig, serverLogger);
            
            var config = new ApplicationConfig
            {
                PublicDomainNameOrAddress = "127.0.0.1",
                ListenPorts = string.Join(",", gamePorts.Select(p => $"{p}/udp")),
                BindToPortHttp = 7000,
                MaxPacketSize = 300,
                BasePacketBufferSize = 64,
                SendTickTimeMs = 20,
                SocketTickTimeMs = 10,
                SocketType = SocketType.BareSocket,
                ReceiveTickTimeMs = 20,
                IsAuthOn = isAuthOn,
                IsConnectionDdosProtectionOn = false
            };
            var requestSender = new FakeSenderWithGameApplication(null, new Dictionary<byte, object> {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}}, CreateRoomDelegate,  UpdateRoomDelegate);

            var gamePacketSender = new PacketBatchSender(taskSchedulerFactory, config, serverLogger);

            var gameSenderFactory = new ShamanMessageSenderFactory(serializer, config);
            var _roomManager = new Game.Rooms.RoomManager(serverLogger, serializer, config, taskSchedulerFactory,
                _roomControllerFactory, gamePacketSender, gameSenderFactory, Mock.Of<IGameMetrics>());


            //setup game server
            return new GameApplication(
                serverLogger,
                config,
                serializer,
                socketFactory,
                taskSchedulerFactory,
                requestSender,
                _roomManager,
                gamePacketSender,
                Mock.Of<IGameMetrics>(), 
                gameSenderFactory,
                protectionManager);
        }
    }
}