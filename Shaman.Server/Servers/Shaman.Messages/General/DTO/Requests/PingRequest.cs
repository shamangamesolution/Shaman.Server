using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests
{
    public class PingRequest : RequestBase
    {
        public PingRequest() : base(CustomOperationCode.PingRequest, string.Empty)
        {
        }

        protected override void SetMessageParameters()
        {
            IsReliable = false;
            IsOrdered = true;
            IsBroadcasted = false;
        }
        
        protected override void SerializeRequestBody(ISerializer serializer)
        {
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
        }
    }
}