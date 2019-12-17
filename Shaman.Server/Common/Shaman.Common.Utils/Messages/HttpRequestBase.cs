using System;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class HttpRequestBase : ISerializable
    {
        public Guid SessionId { get; set; }
        public string EndPoint { get; set; }

        protected HttpRequestBase(string endpoint)
        {
            EndPoint = endpoint;
        }

        protected abstract void SerializeRequestBody(ITypeWriter typeWriter);

        protected abstract void DeserializeRequestBody(ITypeReader typeReader);

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(SessionId);
            typeWriter.Write(EndPoint);
            SerializeRequestBody(typeWriter);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            SessionId = typeReader.ReadGuid();
            EndPoint = typeReader.ReadString();
            DeserializeRequestBody(typeReader);
        }
    }
}