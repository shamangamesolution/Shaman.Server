using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class UpdateRoomStateResponse : ResponseBase
    {
        public UpdateRoomStateResponse() : base(Messages.CustomOperationCode.UpdateRoomState)
        {
            
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
        }
    }
}