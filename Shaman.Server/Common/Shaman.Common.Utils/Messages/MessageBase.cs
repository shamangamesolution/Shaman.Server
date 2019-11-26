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
    
    public abstract class MessageBase : ISerializable
    {       
        public MessageType Type { get; set; }
        public ushort OperationCode;
        public virtual bool IsReliable => false;
        public virtual bool IsOrdered => false;
        public virtual bool IsBroadcasted => false;
        
        public MessageBase(MessageType type, ushort operationCode)
        {
            Type = type;
            OperationCode = operationCode;
        }
        
        //these are impmented in child classes to serialize/deserialize unique fields
        protected abstract void SerializeBody(ITypeWriter typeWriter);        
        protected abstract void DeserializeBody(ITypeReader typeReader);

        public static ushort GetOperationCode(byte[] param)
        {
            return GetOperationCode(param, 0);
        }
        public static ushort GetOperationCode(byte[] param, int offset)
        {
            return BitConverter.ToUInt16(new byte[2] {param[offset] , param[offset+1]}, 0);
        }
        
        public static MessageType GetMessageType(byte[] param)
        {
            return GetMessageType(param, 0);
        }

        public static MessageType GetMessageType(byte[] param, int offset)
        {
            return (MessageType) param[2 + offset];
        }

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(OperationCode);
            typeWriter.Write((byte)Type);
            
            SerializeBody(typeWriter);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            OperationCode = typeReader.ReadUShort();
            Type = (MessageType) typeReader.ReadByte();
            
            DeserializeBody(typeReader);
        }
    }
}