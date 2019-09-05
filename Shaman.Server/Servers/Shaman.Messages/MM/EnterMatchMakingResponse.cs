using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class EnterMatchMakingResponse : ResponseBase
    {
        public MatchMakingErrorCode MatchMakingErrorCode { get; set; }
        
        public EnterMatchMakingResponse(MatchMakingErrorCode matchMakingResultCode = MatchMakingErrorCode.OK) 
            : base(Messages.CustomOperationCode.EnterMatchMaking)
        {
            MatchMakingErrorCode = matchMakingResultCode;
        }

        public EnterMatchMakingResponse() : base(Messages.CustomOperationCode.EnterMatchMaking)
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