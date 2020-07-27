using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Responses.Router
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