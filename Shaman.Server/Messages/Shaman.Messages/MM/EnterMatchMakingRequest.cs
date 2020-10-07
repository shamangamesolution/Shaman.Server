using System.Collections.Generic;
using Shaman.Messages.Helpers;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class EnterMatchMakingRequest : RequestBase
    {
        public Dictionary<byte, object> MatchMakingProperties { get; set; }
        public int MatchMakingWeight { get; set; }

        public EnterMatchMakingRequest(Dictionary<byte, object> properties, int matchMakingWeight = 1) : base(Messages.ShamanOperationCode.EnterMatchMaking)
        {
            MatchMakingProperties = properties;
            MatchMakingWeight = matchMakingWeight;
        }

        public EnterMatchMakingRequest() : base(Messages.ShamanOperationCode.EnterMatchMaking)
        {
            
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteDictionary(MatchMakingProperties, typeWriter.Write);
            typeWriter.Write(MatchMakingWeight);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            MatchMakingProperties = typeReader.ReadDictionary<byte>(typeReader.ReadByte);
            MatchMakingWeight = typeReader.ReadInt();
        }
    }
}