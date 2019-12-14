using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class CreateRoomFromClientRequest : RequestBase
    {
        public Dictionary<byte, object> MatchMakingProperties { get; set; }

        public CreateRoomFromClientRequest(Dictionary<byte, object> matchMakingProperties)
            :this()
        {
            MatchMakingProperties = matchMakingProperties;
        }
        
        public CreateRoomFromClientRequest(): base(CustomOperationCode.CreateRoomFromClient, string.Empty)
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