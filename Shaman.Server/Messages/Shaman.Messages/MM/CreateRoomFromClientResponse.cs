using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Extensions;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class CreateRoomFromClientResponse : ResponseBase
    {
        public JoinInfo JoinInfo { get; set; }

        public CreateRoomFromClientResponse(JoinInfo joinInfo)
            :this()
        {
            JoinInfo = joinInfo;
        }
        
        public CreateRoomFromClientResponse() : base(Messages.ShamanOperationCode.CreateRoomFromClientResponse)
        {
            
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteEntity(JoinInfo);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            JoinInfo = typeReader.ReadEntity<JoinInfo>();
        }
    }
}