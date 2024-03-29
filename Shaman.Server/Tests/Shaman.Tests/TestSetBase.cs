using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Common.Http;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Game;
using Shaman.LiteNetLibAdapter;
using Shaman.Messages.General.DTO.Responses.Auth;
using Shaman.Messages.RoomFlow;
using Shaman.Routing.Balancing.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Tests
{
    public class FakeSender : IRequestSender
    {        
        public async Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request) where T : HttpResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
                return new CreateRoomResponse(Guid.NewGuid()) as T;

            if (typeof(T) == typeof(GetServerInfoListResponse))
                return CreateGetServerInfoListResponse<T>() as T;

            if (typeof(T) == typeof(ValidateSessionIdResponse))
                return new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T;
            
            return new T();
        }

        public async Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
                callback(new CreateRoomResponse(Guid.NewGuid()) as T);
            else

            if (typeof(T) == typeof(GetServerInfoListResponse))
                callback(CreateGetServerInfoListResponse<T>() as T);
            else if (typeof(T) == typeof(ValidateSessionIdResponse))
                callback(new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T);
            else
                callback(new T());
        }

        internal static GetServerInfoListResponse CreateGetServerInfoListResponse<T>() where T : HttpResponseBase, new()
        {
            return new GetServerInfoListResponse(
                new EntityDictionary<ServerInfo>(new List<ServerInfo>{new ServerInfo
                {
                    Address = "", Id = 1, HttpPort = 5555, ServerRole = ServerRole.BackEnd, IsApproved = true
                }}));
        }
    }
    
    public class FakeSenderWithGameApplication : IRequestSender
    {
        private Func<Dictionary<byte, object>, GameApplication, Guid, Guid> _createRoomDelegate;
        private Action<Guid, GameApplication> _updateRoomDelegate;

        private readonly GameApplication _gameApplication;
        private Dictionary<byte, object> _roomProperties;
        
        public FakeSenderWithGameApplication(GameApplication gameApplication,  Dictionary<byte, object> roomProperties, Func<Dictionary<byte, object>, GameApplication, Guid, Guid> createRoomDelegate, Action<Guid, GameApplication> updateRoomDelegate)
        {
            _gameApplication = gameApplication;
            _roomProperties = roomProperties;
            _createRoomDelegate = createRoomDelegate;
            _updateRoomDelegate = updateRoomDelegate;
        }
        
        public async Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request) where T : HttpResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
            {
                
                var roomId = _createRoomDelegate(_roomProperties, _gameApplication, ((CreateRoomRequest)request).RoomId);
                return new CreateRoomResponse(roomId) as T;
            }
            if (typeof(T) == typeof(UpdateRoomResponse))
            {
                var req = request as UpdateRoomRequest;
                _updateRoomDelegate(req.RoomId, _gameApplication);
                return new UpdateRoomResponse() as T;
            }
            
            if (typeof(T) == typeof(GetServerInfoListResponse))
                return FakeSender.CreateGetServerInfoListResponse<T>() as T;;
            
            return new T();
        }

        public async Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
            {
                var roomId = _createRoomDelegate(_roomProperties, _gameApplication, ((CreateRoomRequest)request).RoomId);
                callback(new CreateRoomResponse(roomId) as T);
            }
            else
            if (typeof(T) == typeof(GetServerInfoListResponse))
                callback(FakeSender.CreateGetServerInfoListResponse<T>() as T);
            else
            if (typeof(T) == typeof(ValidateSessionIdResponse))
                callback(new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T);
            else
                callback(new T());
        }
    }
    [TestFixture]
    public class TestSetBase
    {
        //protected List<MessageBase> _receivedMessages = new List<MessageBase>();
        protected IShamanLogger _clientLogger = new ConsoleLogger("C ", LogLevel.Error | LogLevel.Info);
        protected IShamanLogger _serverLogger = new ConsoleLogger("S ", LogLevel.Error | LogLevel.Info);
        protected ISerializer serializer;
        
        //switch Sockets implementation.BEGIN
        protected ISocketFactory socketFactory = new LiteNetSockFactory();
        //switch sockets implementation.END
        
        protected ITaskSchedulerFactory taskSchedulerFactory = null;
        
        private object _syncCollection = new object();
        protected Task EmptyTask = new Task(() => {});

        public TestSetBase()
        {
            serializer = new BinarySerializer();
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
        }
        
    }
}