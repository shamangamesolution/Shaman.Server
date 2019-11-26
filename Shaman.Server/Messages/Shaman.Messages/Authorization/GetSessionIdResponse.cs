using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.Authorization
{
    public class GetSessionIdResponse : ResponseBase
    {
        public string SessionId { get; private set; }
        
        public GetSessionIdResponse() 
            : base(Messages.CustomOperationCode.GetSessionId)
        {
        }
        
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(SessionId);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            this.SessionId = typeReader.ReadString();
        }
    }
}