using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Requests
{
    public class GetCurrentStorageVersionRequest : HttpRequestBase
    {
        public GetCurrentStorageVersionRequest()
         :base(SampleBackEndEndpoints.GetStorageVersion)
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
