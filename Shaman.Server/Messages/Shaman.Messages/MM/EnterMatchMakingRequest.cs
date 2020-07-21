using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General;

namespace Shaman.Messages.MM
{
    public class EnterMatchMakingRequest : RequestBase
    {
        public Dictionary<byte, object> MatchMakingProperties { get; set; }

        public EnterMatchMakingRequest(Dictionary<byte, object> properties) : base(Messages.ShamanOperationCode.EnterMatchMaking)
        {
            MatchMakingProperties = properties;
        }

        public EnterMatchMakingRequest() : base(Messages.ShamanOperationCode.EnterMatchMaking)
        {
            
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteDictionary(MatchMakingProperties, typeWriter.Write);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            MatchMakingProperties = typeReader.ReadDictionary<byte>(typeReader.ReadByte);
        }
    }
}