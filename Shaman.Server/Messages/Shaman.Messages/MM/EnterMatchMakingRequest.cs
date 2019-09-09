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

        public EnterMatchMakingRequest(Dictionary<byte, object> properties) : base(Messages.CustomOperationCode.EnterMatchMaking)
        {
            MatchMakingProperties = properties;
        }

        public EnterMatchMakingRequest() : base(Messages.CustomOperationCode.EnterMatchMaking)
        {
            
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.WriteDictionary(MatchMakingProperties);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            MatchMakingProperties = serializer.ReadDictionary();
        }
    }
}