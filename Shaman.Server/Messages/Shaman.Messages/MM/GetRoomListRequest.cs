using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class GetRoomListRequest : RequestBase
    {
        public Dictionary<byte, object> MatchMakingProperties { get; set; }

        public GetRoomListRequest(): base(CustomOperationCode.GetRoomList)
        {
            
        }
        
        public GetRoomListRequest(Dictionary<byte, object> properties) 
            :this()
        {
            MatchMakingProperties = properties;
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