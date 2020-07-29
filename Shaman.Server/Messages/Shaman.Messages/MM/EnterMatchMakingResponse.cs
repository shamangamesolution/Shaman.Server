using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class EnterMatchMakingResponse : ResponseBase
    {
        public MatchMakingErrorCode MatchMakingErrorCode { get; set; }
        
        public EnterMatchMakingResponse(MatchMakingErrorCode matchMakingResultCode = MatchMakingErrorCode.OK) 
            : base(Messages.ShamanOperationCode.EnterMatchMakingResponse)
        {
            MatchMakingErrorCode = matchMakingResultCode;
        }

        public EnterMatchMakingResponse() : base(Messages.ShamanOperationCode.EnterMatchMakingResponse)
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