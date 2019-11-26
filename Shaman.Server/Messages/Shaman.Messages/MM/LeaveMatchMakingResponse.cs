using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class LeaveMatchMakingResponse : ResponseBase
    {
        public MatchMakingErrorCode MatchMakingErrorCode { get; set; }
        
        public LeaveMatchMakingResponse(MatchMakingErrorCode matchMakingResultCode = MatchMakingErrorCode.OK) 
            : base(Messages.CustomOperationCode.LeaveMatchMaking)
        {
            MatchMakingErrorCode = matchMakingResultCode;
        }

        public LeaveMatchMakingResponse() : base(Messages.CustomOperationCode.LeaveMatchMaking)
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