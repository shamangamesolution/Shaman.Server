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

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.WriteByte((byte)MatchMakingErrorCode);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            this.MatchMakingErrorCode = (MatchMakingErrorCode)serializer.ReadByte();
        }
    }
}