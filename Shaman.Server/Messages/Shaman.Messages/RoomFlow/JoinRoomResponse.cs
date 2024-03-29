using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.RoomFlow
{
    public class JoinRoomResponse : ResponseBase
    {
        public JoinRoomResponse() 
            : base(Messages.ShamanOperationCode.JoinRoomResponse)
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