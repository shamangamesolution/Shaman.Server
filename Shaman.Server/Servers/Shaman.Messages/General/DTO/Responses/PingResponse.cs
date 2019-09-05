using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses
{
    public class PingResponse : ResponseBase
    {
        public PingResponse() : base(CustomOperationCode.PingRequest)
        {
        }

        protected override void SetMessageParameters()
        {
            IsReliable = false;
            IsOrdered = true;
            IsBroadcasted = false;
        }
        
        protected override void SerializeResponseBody(ISerializer serializer)
        {
            
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            
        }
    }
}