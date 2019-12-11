using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Router
{
    public class GetBundleUriResponse : ResponseBase
    {
        public GetBundleUriResponse() : base(CustomOperationCode.ActualizeServer)
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