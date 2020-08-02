using System;
using System.Collections.Concurrent;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client.Peers.MessageHandling
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<ushort, ConcurrentDictionary<Guid, EventHandler>> _handlers = new ConcurrentDictionary<ushort, ConcurrentDictionary<Guid, EventHandler>>();
        private readonly ConcurrentDictionary<Type, ushort> _opCodesMap = new ConcurrentDictionary<Type, ushort>();
        private readonly ConcurrentDictionary<ushort, Func<byte[], int, int, MessageBase>> _parsers =  new ConcurrentDictionary<ushort, Func<byte[], int, int, MessageBase>>();
        private readonly ConcurrentDictionary<Guid, ushort> _handlerIdToOperationCodes = new ConcurrentDictionary<Guid, ushort>();

        public MessageHandler(IShamanLogger logger, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }

        public Guid RegisterOperationHandler<T>(Action<T> handler,
            bool callOnce = false) where T : MessageBase, new()
        {
            var id = Guid.NewGuid();
            
            var operationCode = GetOperationCode<T>();

            _logger.Debug(
                $"Registering OperationHandler {handler.Method} for operation {operationCode} (callOnce = {callOnce})");

            if (!_handlers.TryGetValue(operationCode, out var eventHandlers))
            {
                eventHandlers = new ConcurrentDictionary<Guid, EventHandler>();
                _handlers.TryAdd(operationCode, eventHandlers);
            }
            
            if (!_parsers.ContainsKey(operationCode))
                _parsers.TryAdd(operationCode, (data, offset, length) =>
                    _serializer.DeserializeAs<T>(data, offset, length));

            eventHandlers.TryAdd(id, new EventHandler(msgBase => handler((T) msgBase), callOnce));
            _handlerIdToOperationCodes[id] = operationCode;
            return id;
        }

        public bool UnregisterOperationHandler(Guid id)
        {
            if (!_handlerIdToOperationCodes.TryRemove(id, out var operationCode))
                return false;

            _logger.Debug($"Unregistering OperationHandler {id} for operation {operationCode}");

            if (!_handlers.TryGetValue(operationCode, out var eventHandlers))
                return false;

            return eventHandlers.TryRemove(id, out _);
        }

        private ushort GetOperationCode<T>() where T : MessageBase, new()
        {
            if (!_opCodesMap.TryGetValue(typeof(T), out var operationCode))
            {
                operationCode = (new T()).OperationCode;
                _opCodesMap.TryAdd(typeof(T), operationCode);
            }

            return operationCode;
        }

        private MessageBase DeserializeMessage(ushort operationCode, byte[] buffer, int offset, int length)
        {
            if (!_parsers.TryGetValue(operationCode, out var parser))
                throw new MessageHandleException($"No parser registered for operationCode {operationCode}");
            return parser(buffer, offset, length);
        }

        public bool ProcessMessage(ushort operationCode, byte[] buffer, int offset, int length)
        {
            MessageBase messageBase = null;

            if (!_handlers.TryGetValue(operationCode, out var eventHandlers))
            {
                var msg = $"No handler for message {operationCode}";
                _logger.Debug(msg);
                return false;
            }

            foreach(var item in eventHandlers)
            {
                try
                {
                    if (item.Value.CallOnce && !UnregisterOperationHandler(item.Key))
                        continue;
                    if (messageBase == null)
                    {
                        messageBase = DeserializeMessage(operationCode, buffer, offset, length);
                    }
                    item.Value.Handler.Invoke(messageBase);
                }
                catch (Exception ex)
                {
                    string targetName = item.Value == null ? "" : item.Value.Handler.Method.ToString();
                    var msg =
                        $"ClientOnPackageReceived error: processing message {operationCode} in handler {targetName} {ex}";
                    throw new MessageHandleException(msg, ex);
                }
            }
            return true;
        }

    }
}