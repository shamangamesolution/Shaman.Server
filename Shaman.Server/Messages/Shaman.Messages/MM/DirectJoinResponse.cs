using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Extensions;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class DirectJoinResponse : ResponseBase
    {
        public JoinInfo JoinInfo { get; set; }

        public DirectJoinResponse(JoinInfo joinInfo)
            :this()
        {
            JoinInfo = joinInfo;
        }
        
        public DirectJoinResponse() : base(Messages.ShamanOperationCode.DirectJoinResponse)
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