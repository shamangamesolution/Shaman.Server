using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class LeaveMatchMakingResponse : ResponseBase
    {
        public MatchMakingErrorCode MatchMakingErrorCode { get; set; }

        public LeaveMatchMakingResponse() : base(CustomOperationCode.LeaveMatchMakingResponse)
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