using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

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