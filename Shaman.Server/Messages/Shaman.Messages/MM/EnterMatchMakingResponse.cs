using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class EnterMatchMakingResponse : ResponseBase
    {
        public MatchMakingErrorCode MatchMakingErrorCode { get; set; }
        
        public EnterMatchMakingResponse(MatchMakingErrorCode matchMakingResultCode = MatchMakingErrorCode.OK) 
            : base(Messages.CustomOperationCode.EnterMatchMakingResponse)
        {
            MatchMakingErrorCode = matchMakingResultCode;
        }

        public EnterMatchMakingResponse() : base(Messages.CustomOperationCode.EnterMatchMakingResponse)
        {
            
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write((byte)MatchMakingErrorCode);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            this.MatchMakingErrorCode = (MatchMakingErrorCode)typeReader.ReadByte();
        }
    }
}