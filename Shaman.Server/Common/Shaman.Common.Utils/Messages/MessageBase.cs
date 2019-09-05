using System;
using System.IO;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public enum MessageType : byte
    {
        Event = 1,
        Request = 2,
        Response = 3
    }
    
    public abstract class MessageBase : SerializableBase
    {       
        public MessageType Type { get; set; }
        public ushort OperationCode;
        public bool IsReliable { get; set; }
        public bool IsOrdered { get; set; }
        public bool IsBroadcasted { get; set; }
        public int MessageSizeInBytes { get; set; }
        
        public MessageBase(MessageType type, ushort operationCode)
        {
            Type = type;
            OperationCode = operationCode;
            //call this to setup message parameters on create
            SetMessageParameters();
        }
        
        //these are impmented in child classes to serialize/deserialize unique fields
        protected abstract void SetMessageParameters();
        protected abstract void SerializeBody(ISerializer serializer);        
        protected abstract void DeserializeBody(ISerializer serializer);
        
        //these are for all children serialization/deserialization
        public override byte[] Serialize(ISerializer serializer)
        {
            serializer.WriteUShort(OperationCode);
            serializer.WriteByte((byte)Type);
            
            SerializeBody(serializer);
            var buffer = serializer.GetDataBuffer();
            serializer.GetLogger().Debug($"Serialized message {this.GetType()}. Current buffer size {serializer.GetCurrentBufferSize()}");
            if (ToFlush)
                serializer.Flush();
            return buffer;
        }
        
        private void Deserialize(ISerializer serializer, byte[] param)
        {
            var stream = new MemoryStream(param, 0, param.Length, true);
            serializer.SetStream(stream);
            //call this to setup message parameters after message was deserialized
            SetMessageParameters();
            this.OperationCode = serializer.ReadUShort();
            this.Type = (MessageType) serializer.ReadByte();
            
            this.DeserializeBody(serializer);
            
            this.MessageSizeInBytes = param.Length;
        }
        
        public static T DeserializeAs<T>(ISerializerFactory serializerFactory, byte[] param)
            where T : MessageBase, new()
        {
            var result = new T();
            var serializer = result.GetSerializer(serializerFactory);          
            result.Deserialize(serializer, param);
            return result;
        }

        public static ushort GetOperationCode(byte[] param)
        {
            return BitConverter.ToUInt16(new byte[2] {param[0] , param[1]}, 0);;
        }
        
        public static MessageType GetMessageType(byte[] param)
        {
            return (MessageType)param[2];
        }
        
        
        public void AssertType<T1>() where T1: MessageBase
        {
            if (!(this is T1))
                throw new Exception($"MessageBase cast error: {this.GetType()} is not {typeof(T1)}");
        }
    }
}