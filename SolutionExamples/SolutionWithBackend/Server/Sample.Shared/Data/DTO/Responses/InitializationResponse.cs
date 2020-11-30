using System;
using Sample.Shared.Data.Entity;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Responses
{
    [Serializable]
    public class InitializationResponse : ResponseWithPlayer
    {
        
        public Guid SessionId { get; set; }
        
        public InitializationResponse()
        {

        }

        public InitializationResponse(Player player, Guid sessionId)
            : base(player)
        {
            SessionId = sessionId;
        }

        

        protected override void SerializeResponseWithPlayerBody(ITypeWriter serializer)
        {
            serializer.Write(SessionId.ToByteArray());
        }

        protected override void DeserializeResponseWithPlayerBody(ITypeReader serializer)
        {
            SessionId = new Guid(serializer.ReadBytes());
        }
    }
}
