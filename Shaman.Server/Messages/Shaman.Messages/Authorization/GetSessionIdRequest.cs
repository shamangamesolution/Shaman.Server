using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.Authorization
{
    public class GetSessionIdRequest : RequestBase
    {
        public string AuthToken { get; private set; }
        
        public GetSessionIdRequest() 
            : base(Messages.CustomOperationCode.GetSessionId)
        {
        }
        
        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(AuthToken);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            this.AuthToken = typeReader.ReadString();
        }
    }
}