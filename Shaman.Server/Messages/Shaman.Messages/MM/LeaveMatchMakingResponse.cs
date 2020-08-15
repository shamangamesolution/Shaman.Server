using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class LeaveMatchMakingResponse : ResponseBase
    {
        public MatchMakingErrorCode MatchMakingErrorCode { get; set; }

        public LeaveMatchMakingResponse() : base(ShamanOperationCode.LeaveMatchMakingResponse)
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