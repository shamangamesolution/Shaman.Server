using System;
using System.Collections.Generic;
using Moq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Game;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.LiteNetLibAdapter;
using Shaman.Messages;
using Shaman.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Serialization;
using Shaman.Tests.Configuration;
using Shaman.Tests.GameModeControllers;
using Shaman.Tests.Providers;
using Shaman.TestTools.Events;

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
        
        public static MmApplication GetMm(ushort mmPort, ushort gamePort, GameApplication gameApplication, int maximumPlayers = 2, int mmTime = 10000, int ddosConnectionsLevel = 300, int ddosConnectionCheckInterval = 5000)
        {
            var socketFactory = new LiteNetSockFactory();
            var serializer = new BinarySerializer();
            var serverLogger = new ConsoleLogger("M ", LogLevel.Error | LogLevel.Info | LogLevel.Debug);

            var config = new ApplicationConfig
            {
                PublicDomainNameOrAddress = "127.0.0.1",
                ListenPorts = new List<ushort> {mmPort},
                BindToPortHttp = 7002,
                MaxPacketSize = 300,
                BasePacketBufferSize = 64,
                SendTickTimeMs = 20,
                SocketTickTimeMs = 10,
                SocketType = SocketType.BareSocket,
                ReceiveTickTimeMs = 20,
                IsConnectionDdosProtectionOn = false
            };
            var roomPropertiesProvider = new FakeRoomPropertiesProvider3(250, maximumPlayers, mmTime);
            var taskSchedulerFactory = new TaskSchedulerFactory(serverLogger);
            var requestSender = new FakeSenderWithGameApplication(gameApplication,  new Dictionary<byte, object> {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}}, CreateRoomDelegate,  UpdateRoomDelegate);

            var _mmPacketSender = new PacketBatchSender(taskSchedulerFactory, config, serverLogger);
            
            var _playerManager = new PlayersManager( Mock.Of<IMmMetrics>(), serverLogger);

            //_serverProvider = new MatchMakerServerInfoProvider(requestSender, taskSchedulerFactory, config, _serverLogger, _statsProvider);
            var _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", $"{gamePort}");
            var roomApiProvider = new DefaultRoomApiProvider(requestSender, serverLogger);
            var _mmRoomManager =
                new MM.Managers.RoomManager(_serverProvider, serverLogger, taskSchedulerFactory, roomApiProvider);

            var sender = GetSHamanMessageSender(serializer, _mmPacketSender, config, serverLogger);
            var _mmGroupManager = new MatchMakingGroupManager(serverLogger, taskSchedulerFactory, _playerManager,
                sender, Mock.Of<IMmMetrics>(), _mmRoomManager, roomPropertiesProvider, config);
            
            var matchMaker = new MatchMaker(_playerManager,_mmGroupManager);
            //
            // var _measures = new Dictionary<byte, object>();
            // _measures.Add(FakePropertyCodes.PlayerProperties.Level, 1);
            // matchMaker.AddMatchMakingGroup(_measures);
            matchMaker.AddRequiredProperty(FakePropertyCodes.PlayerProperties.Level);

            var senderFactory = new ShamanMessageSenderFactory(serializer, config);
            var protectionManagerConfig = new ConnectionDdosProtectionConfig(ddosConnectionsLevel, ddosConnectionCheckInterval, 5000, 60000);
            var connectionDdosProtection = new ConnectDdosProtection(protectionManagerConfig,taskSchedulerFactory, serverLogger);
            var protectionManager = new ProtectionManager(connectionDdosProtection, protectionManagerConfig, serverLogger);
            //setup mm server
            return new MmApplication(serverLogger, config, serializer, socketFactory, matchMaker,
                requestSender, taskSchedulerFactory, _mmPacketSender,senderFactory, _serverProvider, _mmRoomManager,
                _mmGroupManager, _playerManager, Mock.Of<IMmMetrics>(),protectionManager);
        }

        public static GameApplication GetGame(ushort gamePort, bool isAuthOn = false)
        {
            return GetGame(new List<ushort> {gamePort}, isAuthOn);
        }
        
        public static GameApplication GetGame(List<ushort> gamePorts, bool isAuthOn = false)
        {
            var _roomControllerFactory = new FakeRoomControllerFactory();
            var serverLogger = new ConsoleLogger("G ", LogLevel.Error | LogLevel.Info | LogLevel.Debug);
            var socketFactory = new LiteNetSockFactory();
            var serializer = new BinarySerializer();
            var taskSchedulerFactory = new TaskSchedulerFactory(serverLogger);
            var protectionManagerConfig = new ConnectionDdosProtectionConfig(300, 5000, 5000, 60000);
            var connectionDdosProtection = new ConnectDdosProtection(protectionManagerConfig,taskSchedulerFactory, serverLogger);
            var protectionManager = new ProtectionManager(connectionDdosProtection, protectionManagerConfig, serverLogger);
            
            var config = new ApplicationConfig
            {
                PublicDomainNameOrAddress = "127.0.0.1",
                ListenPorts = gamePorts,
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
                _roomControllerFactory, gamePacketSender, gameSenderFactory, Mock.Of<IGameMetrics>(), new RoomStateUpdaterStub());


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