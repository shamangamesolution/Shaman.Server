
using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.Authorization
{
    public class AuthorizationRequest : RequestBase
    {
        public int BackendId { get; set; }
        
        public AuthorizationRequest(Guid sessionId) 
            : base(Messages.CustomOperationCode.Authorization)
        {
            this.SessionId = sessionId;
        }
        
        public AuthorizationRequest(int backendId, Guid sessionId) 
            : base(Messages.CustomOperationCode.Authorization)
        {
            this.SessionId = sessionId;
            this.BackendId = backendId;
        }
        
        public AuthorizationRequest() 
            : base(Messages.CustomOperationCode.Authorization)
        {
        }
        
        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.WriteInt(BackendId);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            this.BackendId = serializer.ReadInt();
        }
    }
}