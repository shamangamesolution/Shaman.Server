using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses
{
    public class ErrorResponse : ResponseBase
    {
        public ResultCode ErrorCode { get; set; }
        
        public ErrorResponse(ResultCode errorCode) 
            : base(Messages.CustomOperationCode.EnterMatchMaking)
        {
            ErrorCode = errorCode;
        }
        
        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.WriteUShort((byte)ErrorCode);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            this.ErrorCode = (ResultCode)serializer.ReadUShort();
        }
    }
}