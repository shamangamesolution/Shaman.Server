using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Requests
{
    public class GetPlayerGameDataRequest : HttpRequestBase
    {
        public GetPlayerGameDataRequest() : base(SampleBackEndEndpoints.GetPlayerGameData)
        {
        }

        protected override void SerializeRequestBody(ITypeWriter serializer)
        {
            
        }

        protected override void DeserializeRequestBody(ITypeReader serializer)
        {
            
        }
    }
}