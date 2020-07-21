using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses
{
    public class ErrorResponse : ResponseBase
    {
        public ResultCode ErrorCode { get; set; }
        
        public ErrorResponse() 
            : base(ShamanOperationCode.Error)
        {
        }
        public ErrorResponse(ResultCode errorCode) 
            : base(ShamanOperationCode.Error)
        {
            ErrorCode = errorCode;
        }
        
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write((ushort)ErrorCode);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            this.ErrorCode = (ResultCode)typeReader.ReadUShort();
        }
    }
}