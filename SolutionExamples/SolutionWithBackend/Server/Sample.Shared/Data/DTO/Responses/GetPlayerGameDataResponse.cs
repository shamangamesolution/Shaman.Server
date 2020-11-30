using Sample.Shared.Data.Entity;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Sample.Shared.Data.DTO.Responses
{
    public class GetPlayerGameDataResponse : HttpResponseBase
    {
        public Player Player { get; set; }
        
        public GetPlayerGameDataResponse() 
        {
        }

        public GetPlayerGameDataResponse(Player player) 
        {
            Player = player;
        }

        protected override void SerializeResponseBody(ITypeWriter serializer)
        {
            serializer.WriteEntity<Player>(Player);
        }

        protected override void DeserializeResponseBody(ITypeReader serializer)
        {
            Player = serializer.ReadEntity<Player>();
        }
    }
}