using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class GetServerInfoListRequest : HttpRequestBase
    {
        public bool ActualOnly { get; set; }

        public GetServerInfoListRequest() : base(BackEndEndpoints.GetServerInfoList)
        {
            
        }
        
        public GetServerInfoListRequest(bool actualOnly = false) : this()
        {
            ActualOnly = actualOnly;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ActualOnly);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ActualOnly = typeReader.ReadBool();
        }
    }
}