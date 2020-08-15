using System;
using System.Collections.Generic;
using Shaman.Messages.Helpers;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class DirectJoinRequest : RequestBase
    {
        public Guid RoomId { get; set; }
        public Dictionary<byte, object> MatchMakingProperties { get; set; }

        public DirectJoinRequest(Guid roomId, Dictionary<byte, object> matchMakingProperties)
            :this()
        {
            RoomId = roomId;
            MatchMakingProperties = matchMakingProperties;
        }
        
        public DirectJoinRequest(): base(ShamanOperationCode.DirectJoin)
        {
            
        }


        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.WriteDictionary(MatchMakingProperties, typeWriter.Write);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            MatchMakingProperties = typeReader.ReadDictionary<byte>(typeReader.ReadByte);
        }
    }
}