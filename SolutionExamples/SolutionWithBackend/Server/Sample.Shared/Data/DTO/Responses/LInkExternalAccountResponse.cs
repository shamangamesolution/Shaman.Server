using Sample.Shared.Data.Entity;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Responses
{
    public class LinkExternalAccountResponse : ResponseWithPlayer
    {
        public LinkExternalAccountResponse() 
        {
        }

        public LinkExternalAccountResponse(Player player) : base(player)
        {
        }

        protected override void SerializeResponseWithPlayerBody(ITypeWriter serializer)
        {
        }

        protected override void DeserializeResponseWithPlayerBody(ITypeReader serializer)
        {
        }
    }
}