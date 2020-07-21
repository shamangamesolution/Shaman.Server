using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.RoomFlow;

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