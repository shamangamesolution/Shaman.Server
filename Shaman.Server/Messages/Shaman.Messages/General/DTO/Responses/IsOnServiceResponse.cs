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

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.Write(this.IsOnService);
            serializer.Write(this.ClientVersion);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            IsOnService = serializer.ReadBool();
            ClientVersion = serializer.ReadString();
        }
    }
}
