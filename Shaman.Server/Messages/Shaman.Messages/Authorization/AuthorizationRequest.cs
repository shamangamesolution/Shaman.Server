
using System;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Authorization
{
    public class AuthorizationRequest : RequestBase
    {
        public int BackendId { get; set; }
        
        public AuthorizationRequest(Guid sessionId) 
            : base(Messages.ShamanOperationCode.Authorization)
        {
            this.SessionId = sessionId;
        }
        
        public AuthorizationRequest(int backendId, Guid sessionId) 
            : base(Messages.ShamanOperationCode.Authorization)
        {
            this.SessionId = sessionId;
            this.BackendId = backendId;
        }
        
        public AuthorizationRequest() 
            : base(Messages.ShamanOperationCode.Authorization)
        {
        }
        
        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(BackendId);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            this.BackendId = typeReader.ReadInt();
        }
    }
}