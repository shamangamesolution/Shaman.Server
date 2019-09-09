using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Responses
{
    public abstract class ResponseWithPlayer : ResponseBase
    {
        public Player Player { get; set; }
        public SerializationRules SerializationRules { get; set; }

        protected ResponseWithPlayer(ushort operationCode)
            :base(operationCode)
        {
        }
        
        protected ResponseWithPlayer(ushort operationCode, SerializationRules serializationRules, Player player)
            :base(operationCode)
        {
            SerializationRules = serializationRules;
            Player = player;
        }

        protected abstract void SerializeResponseWithPlayerBody(ISerializer serializer);
        
        protected abstract void DeserializeResponseWithPlayerBody(ISerializer serializer);

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.Write((short)SerializationRules);
            serializer.WritePlayer(Player, SerializationRules);
            SerializeResponseWithPlayerBody(serializer);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            SerializationRules = (SerializationRules)serializer.ReadShort();
            Player = serializer.ReadPlayer();
            DeserializeResponseWithPlayerBody(serializer);
        }
    }
}
