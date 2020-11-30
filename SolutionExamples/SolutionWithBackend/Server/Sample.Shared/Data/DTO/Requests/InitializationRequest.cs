using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Requests
{
    [Serializable]
    public class InitializationRequest : HttpRequestBase
    {
        public Guid AuthToken { get; set; }
        public string GuestId { get; set; }
        public Dictionary<int, string> ProviderIds { get; set; }
        
        
        public InitializationRequest()
            :base(SampleBackEndEndpoints.Initialization)
        {
            ProviderIds = new Dictionary<int, string>();
        }
        
        public InitializationRequest(Guid authToken, string guestId, Dictionary<int, string> providerIds)
            :this()
        {
            AuthToken = authToken;
            GuestId = guestId;
            ProviderIds = providerIds;
        }

        protected override void SerializeRequestBody(ITypeWriter serializer)
        {
            serializer.Write(AuthToken.ToByteArray());
            serializer.Write(GuestId);
            serializer.Write((byte)ProviderIds.Count());
            foreach (var item in ProviderIds)
            {
                serializer.Write(item.Key);
                serializer.Write(item.Value);
            }
        }

        protected override void DeserializeRequestBody(ITypeReader serializer)
        {
            AuthToken = new Guid (serializer.ReadBytes());
            GuestId = serializer.ReadString();
            var cnt = serializer.ReadByte();
            ProviderIds = new Dictionary<int, string>();
            for (var i = 0; i < cnt; i++)
            {
                var key = serializer.ReadInt();
                var value = serializer.ReadString();
                ProviderIds.Add(key, value);
            }
        }
    }
}
