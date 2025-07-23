using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Client.Peers.MessageHandling
{
    public class MessageHandler<TOpCode> : IMessageHandler<TOpCode>
    {
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;
        private readonly ConcurrentDictionary<TOpCode, ConcurrentDictionary<Guid, EventHandler<TOpCode>>> _handlers = new();
        private readonly ConcurrentDictionary<Type, TOpCode> _opCodesMap = new();
        private readonly ConcurrentDictionary<TOpCode, Func<byte[], int, int, IOperationCodeProvider<TOpCode>>> _parsers = new();
        private readonly ConcurrentDictionary<Guid, TOpCode> _handlerIdToOperationCodes = new();

        public MessageHandler(IShamanLogger logger, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }

        public Guid RegisterOperationHandler<T>(Action<T, Exception> handler, bool callOnce = false)
            where T : IOperationCodeProvider<TOpCode>, new()
        {
            var id = Guid.NewGuid();
            
            var operationCode = GetOperationCode<T>();

            _logger.Debug(
                $"Registering OperationHandler {handler.Method} for operation {operationCode} (callOnce = {callOnce})");

            if (!_handlers.TryGetValue(operationCode, out var eventHandlers))
            {
                eventHandlers = new ConcurrentDictionary<Guid, EventHandler<TOpCode>>();
                _handlers.TryAdd(operationCode, eventHandlers);
            }
            
            if (!_parsers.ContainsKey(operationCode))
                _parsers.TryAdd(operationCode, (data, offset, length) =>
                    _serializer.DeserializeAs<T>(data, offset, length));

            eventHandlers.TryAdd(id, new EventHandler<TOpCode>((msgBase, err) => handler((T) msgBase, err), callOnce));
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

        private TOpCode GetOperationCode<T>() where T : IOperationCodeProvider<TOpCode>, new()
        {
            if (!_opCodesMap.TryGetValue(typeof(T), out var operationCode))
            {
                operationCode = (new T()).OperationCode;
                _opCodesMap.TryAdd(typeof(T), operationCode);
            }

            return operationCode;
        }

        private IOperationCodeProvider<TOpCode> DeserializeMessage(TOpCode operationCode, byte[] buffer, int offset, int length)
        {
            if (!_parsers.TryGetValue(operationCode, out var parser))
                throw new MessageHandleException($"No parser registered for operationCode {operationCode}");
            return parser(buffer, offset, length);
        }

        private readonly ThreadLocal<List<KeyValuePair<Guid, EventHandler<TOpCode>>>> _handlerIterateBuffers = new();

        public bool ProcessMessage(TOpCode operationCode, byte[] buffer, int offset, int length)
        {
            IOperationCodeProvider<TOpCode> messageBase = null;

            if (!_handlers.TryGetValue(operationCode, out var eventHandlers))
            {
                var msg = $"No handler for message {operationCode}";
                _logger.Debug(msg);
                return false;
            }

            // take a snapshot of handler's set here, before any handler will be executed 
            // using buffer to avoid allocations
            var iterateBuffer = _handlerIterateBuffers.Value ??= new List<KeyValuePair<Guid, EventHandler<TOpCode>>>();
            iterateBuffer.Clear();
            iterateBuffer.AddRange(eventHandlers);

            foreach(var item in iterateBuffer)
            {
                try
                {
                    if (item.Value.CallOnce && !UnregisterOperationHandler(item.Key))
                        continue;
                    messageBase ??= DeserializeMessage(operationCode, buffer, offset, length);
                    item.Value.Handler.Invoke(messageBase, null);
                }
                catch (Exception ex)
                {
                    string targetName = item.Value == null ? "" : item.Value.Handler.Method.ToString();
                    var msg =
                        $"ClientOnPackageReceived error: processing message {operationCode} in handler {targetName} {ex}";
                    try
                    {
                        item.Value.Handler.Invoke(null, ex);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    throw new MessageHandleException(msg, ex);
                }
            }
            return true;
        }
    }
}