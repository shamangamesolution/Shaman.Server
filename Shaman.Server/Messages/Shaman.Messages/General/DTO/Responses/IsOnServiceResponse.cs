using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses
{
    public class IsOnServiceResponse : ResponseBase
    {
        public bool IsOnService { get; set; }
        public string ClientVersion { get; set; }
        
        public IsOnServiceResponse()
            :base(CustomOperationCode.IsOnServiceHttp)
        {

        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(this.IsOnService);
            typeWriter.Write(this.ClientVersion);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            IsOnService = typeReader.ReadBool();
            ClientVersion = typeReader.ReadString();
        }
    }
}
