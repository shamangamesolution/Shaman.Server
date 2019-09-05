using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests
{
    [Serializable]
    public class InitializationRequest : RequestBase
    {
        public Guid AuthToken { get; set; }
        public string GuestId { get; set; }

        public InitializationRequest()
            :base(CustomOperationCode.Initialization, BackEndEndpoints.Initialization)
        {
            
        }
        
        public InitializationRequest(Guid authToken, string guestId)
            :this()
        {
            AuthToken = authToken;
            GuestId = guestId;
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.Write(AuthToken.ToByteArray());
            serializer.Write(GuestId);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            AuthToken = new Guid (serializer.ReadBytes());
            GuestId = serializer.ReadString();
        }
    }
}
