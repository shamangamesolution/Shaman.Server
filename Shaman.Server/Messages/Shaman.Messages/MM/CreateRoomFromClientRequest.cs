using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

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
        
        public CreateRoomFromClientRequest(): base(ShamanOperationCode.CreateRoomFromClient)
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