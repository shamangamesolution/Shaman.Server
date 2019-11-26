using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class LeaveMatchMakingRequest : RequestBase
    {

        public LeaveMatchMakingRequest() : base(Messages.CustomOperationCode.LeaveMatchMaking)
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