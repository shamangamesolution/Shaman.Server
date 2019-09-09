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

        protected override void SerializeRequestBody(ISerializer serializer)
        {

        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {

        }
    }
}