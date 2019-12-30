using System;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class MessageBase : ISerializable
    {       
        public ushort OperationCode;
        public virtual bool IsReliable => false;
        public virtual bool IsOrdered => false;
        public virtual bool IsBroadcasted => false;
        
        public MessageBase(ushort operationCode)
        {
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

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(OperationCode);
            SerializeBody(typeWriter);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            OperationCode = typeReader.ReadUShort();
            DeserializeBody(typeReader);
        }
    }
}