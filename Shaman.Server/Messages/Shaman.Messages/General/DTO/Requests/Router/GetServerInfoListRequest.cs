using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class GetServerInfoListRequest : RequestBase
    {
        public bool ActualOnly { get; set; }

        public GetServerInfoListRequest() : base(CustomOperationCode.GetServerInfoList, BackEndEndpoints.GetServerInfoList)
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