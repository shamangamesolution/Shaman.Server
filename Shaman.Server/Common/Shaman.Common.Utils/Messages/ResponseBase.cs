using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class ResponseBase : MessageBase
    {
        public override bool IsReliable => true;
        
        public ResultCode ResultCode { get; set; }
        public string Message { get; set; }

        
        public ResponseBase(byte operationCode) : base(operationCode)
        {
            ResultCode = ResultCode.OK;
        }

        public bool Success => ResultCode == ResultCode.OK;

        protected abstract void SerializeResponseBody(ITypeWriter typeWriter);
        protected abstract void DeserializeResponseBody(ITypeReader typeReader);
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write((byte)ResultCode);
            typeWriter.Write(Message);
            if (ResultCode == ResultCode.OK)
                SerializeResponseBody(typeWriter);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            this.ResultCode = (ResultCode) typeReader.ReadByte();
            this.Message = typeReader.ReadString();
            if (ResultCode == ResultCode.OK)
                DeserializeResponseBody(typeReader);
        }

        public void SetError(string message)
        {
            ResultCode = ResultCode.RequestProcessingError;
            Message = message;
        }
    }

}