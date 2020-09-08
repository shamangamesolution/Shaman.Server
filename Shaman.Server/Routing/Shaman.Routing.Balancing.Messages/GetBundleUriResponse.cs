using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Balancing.Messages
{
    public class GetBundleUriResponse : HttpResponseBase
    {
        public GetBundleUriResponse() : base()
        {
        }

        public string BundleUri { get; set; }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(BundleUri);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            BundleUri = typeReader.ReadString();
        }
    }
}