using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class JoinRoomResponse : ResponseBase
    {
        public JoinRoomResponse() 
            : base(Messages.CustomOperationCode.JoinRoom)
        {
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            
        }
    }
}