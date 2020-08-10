using System;
using Shaman.Common.Utils.Senders;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;

namespace Shaman.Serialization.Room
{
    public class ShamanRoomSender
    {
        private readonly IRoomSender _roomSender;
        private readonly ISerializer _serializer;
        private readonly ShamanStreamPool _shamanStreamPool;

        public ShamanRoomSender(IRoomSender roomSender, ISerializer serializer)
        {
            _roomSender = roomSender;
            _serializer = serializer;
            _shamanStreamPool = new ShamanStreamPool(64);
        }

        public int Send(ISerializable message, DeliveryOptions deliveryOptions, Guid peer)
        {
            var stream = _shamanStreamPool.Rent(message.GetType());
            try
            {
                _serializer.Serialize(message, stream);
                _roomSender.Send(new Payload(stream.GetBuffer()), deliveryOptions, peer);
                return (int) stream.Length;
            }
            finally
            {
                _shamanStreamPool.Return(stream, message.GetType());
            }
        }

        public int SendToAll(ISerializable message, DeliveryOptions deliveryOptions)
        {
            var stream = _shamanStreamPool.Rent(message.GetType());
            try
            {
                _serializer.Serialize(message, stream);
                _roomSender.SendToAll(new Payload(stream.GetBuffer()), deliveryOptions);
                return (int) stream.Length;
            }
            finally
            {
                _shamanStreamPool.Return(stream, message.GetType());
            }
        }
        public int SendToAll(ISerializable message, DeliveryOptions deliveryOptions, Guid exception)
        {
            var stream = _shamanStreamPool.Rent(message.GetType());
            try
            {
                _serializer.Serialize(message, stream);
                _roomSender.SendToAll(new Payload(stream.GetBuffer()), deliveryOptions, exception);
                return (int) stream.Length;
            }
            finally
            {
                _shamanStreamPool.Return(stream, message.GetType());
            }
        }
    }
}