using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Responses
{
    [Serializable]
    public class InitializationResponse : ResponseWithPlayer
    {
        
        public Guid SessionId { get; set; }
        
        public InitializationResponse()
            : base(CustomOperationCode.Initialization)
        {

        }

        public InitializationResponse(SerializationRules serializationRules, Player player, Guid sessionId)
            : base(CustomOperationCode.Initialization, serializationRules, player)
        {
            SessionId = sessionId;
        }

        

        protected override void SerializeResponseWithPlayerBody(ISerializer serializer)
        {
            serializer.Write(SessionId.ToByteArray());
        }

        protected override void DeserializeResponseWithPlayerBody(ISerializer serializer)
        {
            SessionId = new Guid(serializer.ReadBytes());
        }
    }
}
