using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public enum ResultCode : ushort
    {
        OK = 0,
        UnknownOperation = 1,
        MessageProcessingError = 2,
        SendRequestError = 3,
        RequestProcessingError = 4,
        NotAuthorized = 5
    }
    
    public abstract class ResponseBase : MessageBase
    {
        public ResultCode ResultCode { get; set; }
        public string Message { get; set; }
        
        public ResponseBase(ushort operationCode) : base(MessageType.Response, operationCode)
        {
            ResultCode = ResultCode.OK;
        }

        public bool Success => ResultCode == ResultCode.OK;

        protected abstract void SerializeResponseBody(ISerializer serializer);
        protected abstract void DeserializeResponseBody(ISerializer serializer);
        
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteUShort((byte)ResultCode);
            serializer.WriteString(Message);
            SerializeResponseBody(serializer);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            this.ResultCode = (ResultCode)serializer.ReadUShort();
            this.Message = serializer.ReadString();
            DeserializeResponseBody(serializer);
        }
        
        protected override void SetMessageParameters()
        {
            IsReliable = true;
            IsOrdered = true;
            IsBroadcasted = false;
        }

        public void SetError(string message)
        {
            ResultCode = ResultCode.RequestProcessingError;
            Message = message;
        }
    }
}