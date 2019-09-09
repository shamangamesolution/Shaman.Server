using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.HazelAdapter;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.DTO.Responses.Auth;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests
{
    public class FakeSender : IRequestSender
    {        
        public async Task<T> SendRequest<T>(string url, RequestBase request) where T : ResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
                return new CreateRoomResponse(Guid.NewGuid()) as T;

            if (typeof(T) == typeof(GetBackendsListResponse))
                return new GetBackendsListResponse(new List<Backend> {new Backend(1, "", 5555)}) as T;

            if (typeof(T) == typeof(ValidateSessionIdResponse))
                return new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T;

            if (typeof(T) == typeof(InitializationResponse))
                return new InitializationResponse(SerializationRules.AllInfo, new Player(), Guid.NewGuid()) {ResultCode = ResultCode.OK} as T;
            
            return new T();
        }

        public async Task SendRequest<T>(string url, RequestBase request, Action<T> callback) where T : ResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
                callback(new CreateRoomResponse(Guid.NewGuid()) as T);
            else
            if (typeof(T) == typeof(GetBackendsListResponse))
                callback(new GetBackendsListResponse(new List<Backend> {new Backend(1, "", 5555)}) as T);
            else
            if (typeof(T) == typeof(ValidateSessionIdResponse))
                callback(new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T);
            else         
            if (typeof(T) == typeof(InitializationResponse))
                callback(new InitializationResponse(SerializationRules.AllInfo, new Player(), Guid.NewGuid()) as T);
            else
                callback(new T());
        }
    }
    
    public class FakeSenderWithGameApplication : IRequestSender
    {
        private Func<Dictionary<byte, object>, Guid> _createRoomDelegate;
        private Action<ActualizeServerRequest> _actualizeDelegate;
        
        private Dictionary<byte, object> _roomProperties;
        
        public FakeSenderWithGameApplication( Dictionary<byte, object> roomProperties, Func<Dictionary<byte, object>, Guid> createRoomDelegate, Action<ActualizeServerRequest> actualizeDelegate)
        {
            _roomProperties = roomProperties;
            _createRoomDelegate = createRoomDelegate;
            _actualizeDelegate = actualizeDelegate;
        }
        
        public async Task<T> SendRequest<T>(string url, RequestBase request) where T : ResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
            {
                var roomId = _createRoomDelegate(_roomProperties);
                return new CreateRoomResponse(roomId) as T;
            }
            
            if (typeof(T) == typeof(ActualizeServerResponse))
            {
                _actualizeDelegate(request as ActualizeServerRequest);
                return new ActualizeServerResponse() as T;
            }
            
            if (typeof(T) == typeof(GetBackendsListResponse))
                return new GetBackendsListResponse(new List<Backend> {new Backend(1, "", 5555)}) as T;

//            if (typeof(T) == typeof(ValidateSessionIdResponse))
//                return new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T;
            
            if (typeof(T) == typeof(InitializationResponse))
                return new InitializationResponse(SerializationRules.AllInfo, new Player(), Guid.NewGuid()) as T;
            
            return new T();
        }

        public async Task SendRequest<T>(string url, RequestBase request, Action<T> callback) where T : ResponseBase, new()
        {
            if (typeof(T) == typeof(CreateRoomResponse))
            {
                var roomId = _createRoomDelegate(_roomProperties);
                callback(new CreateRoomResponse(roomId) as T);
            }
            else
            if (typeof(T) == typeof(ActualizeServerResponse))
            {
                _actualizeDelegate(request as ActualizeServerRequest);
                callback(new ActualizeServerResponse() as T);
            }
            else
            if (typeof(T) == typeof(GetBackendsListResponse))
                callback(new GetBackendsListResponse(new List<Backend> {new Backend(1, "", 5555)}) as T);
            else
            if (typeof(T) == typeof(ValidateSessionIdResponse))
                callback(new ValidateSessionIdResponse() {ResultCode = ResultCode.OK} as T);
            else
            if (typeof(T) == typeof(InitializationResponse))
                callback(new InitializationResponse(SerializationRules.AllInfo, new Player(), Guid.NewGuid()) as T);
            else
                callback(new T());
        }
    }
    
    [TestFixture]
    public class TestSetBase
    {
        //protected List<MessageBase> _receivedMessages = new List<MessageBase>();
        protected IShamanLogger _clientLogger = new ConsoleLogger("C ", LogLevel.Error | LogLevel.Info | LogLevel.Debug);
        protected IShamanLogger _serverLogger = new ConsoleLogger("S ", LogLevel.Error );
        protected ISerializerFactory serializerFactory;
        
        //switch Sockets implementation.BEGIN
        protected ISocketFactory socketFactory = new HazelSockFactory();
        //switch sockets implementation.END
        
        protected ITaskSchedulerFactory taskSchedulerFactory = null;
        
        private object _syncCollection = new object();
        protected Task EmptyTask = new Task(() => {});

        public TestSetBase()
        {
            serializerFactory = new SerializerFactory(_serverLogger);
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
        }
        
    }
}