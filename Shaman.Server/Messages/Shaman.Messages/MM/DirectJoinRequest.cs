using System;
using System.Collections.Generic;
using Shaman.Messages.Helpers;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class DirectJoinRequest : RequestBase
    {
        public Guid RoomId { get; set; }
        public Dictionary<byte, object> MatchMakingProperties { get; set; }
        public int MatchMakingWeight { get; set; }
        
        public DirectJoinRequest(Guid roomId, Dictionary<byte, object> matchMakingProperties, int matchMakingWeight = 1)
            :this()
        {
            RoomId = roomId;
            MatchMakingProperties = matchMakingProperties;
            MatchMakingWeight = matchMakingWeight;
        }
        
        public DirectJoinRequest(): base(ShamanOperationCode.DirectJoin)
        {
            
        }


        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.WriteDictionary(MatchMakingProperties, typeWriter.Write);
            typeWriter.Write(MatchMakingWeight);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            MatchMakingProperties = typeReader.ReadDictionary<byte>(typeReader.ReadByte);
            MatchMakingWeight = typeReader.ReadInt();
        }
    }
}