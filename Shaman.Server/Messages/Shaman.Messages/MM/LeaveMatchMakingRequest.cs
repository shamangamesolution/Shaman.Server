using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class LeaveMatchMakingRequest : RequestBase
    {

        public LeaveMatchMakingRequest() : base(Messages.ShamanOperationCode.LeaveMatchMaking)
        {

        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {

        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {

        }
    }
}