using Sample.Shared.Data.Entity;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Sample.Shared.Data.DTO.Responses
{
    public abstract class ResponseWithPlayer : HttpResponseBase
    {
        public Player Player { get; set; }
        private Player _prevPlayer;

        protected ResponseWithPlayer()
        {
        }
        
        protected ResponseWithPlayer(Player player)
        {
            Player = player;
        }

        protected abstract void SerializeResponseWithPlayerBody(ITypeWriter serializer);
        
        protected abstract void DeserializeResponseWithPlayerBody(ITypeReader serializer);

        protected override void SerializeResponseBody(ITypeWriter serializer)
        {
            serializer.WriteEntity(Player);
            SerializeResponseWithPlayerBody(serializer);
        }

        protected override void DeserializeResponseBody(ITypeReader serializer)
        {
            Player = serializer.ReadEntity<Player>();
            DeserializeResponseWithPlayerBody(serializer);
        }
    }
}
