namespace Shaman.Serialization.Messages.Udp
{
    public abstract class MessageBase : ISerializable
    {       
        public byte OperationCode;
        public virtual bool IsReliable => false;
        public virtual bool IsOrdered => false;
        
        public MessageBase(byte operationCode)
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
            return param[offset]; 
        }

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(OperationCode);
            SerializeBody(typeWriter);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            OperationCode = typeReader.ReadByte();
            DeserializeBody(typeReader);
        }
    }
}