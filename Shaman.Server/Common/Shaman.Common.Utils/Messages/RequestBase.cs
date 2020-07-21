using System;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class RequestBase : MessageBase
    {
        public override bool IsReliable => true;

        public Guid SessionId { get; set; }

        protected RequestBase(byte operationCode) : base(operationCode)
        {
        }
        
        protected abstract void SerializeRequestBody(ITypeWriter typeWriter);
        protected abstract void DeserializeRequestBody(ITypeReader typeReader);
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(SessionId);
            SerializeRequestBody(typeWriter);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            this.SessionId = typeReader.ReadGuid();
            DeserializeRequestBody(typeReader);
        }
    }
}