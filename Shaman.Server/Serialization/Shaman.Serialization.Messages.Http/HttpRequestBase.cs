using System;

namespace Shaman.Serialization.Messages.Http
{
    public abstract class HttpRequestBase : HttpSessionRequestBase
    {
        protected HttpRequestBase(string endpoint) : base(endpoint)
        {
            EndPoint = endpoint;
        }

        protected abstract void SerializeRequestBody(ITypeWriter typeWriter);

        protected abstract void DeserializeRequestBody(ITypeReader typeReader);

        public override void Serialize(ITypeWriter typeWriter)
        {
            base.Serialize(typeWriter);
            typeWriter.Write(EndPoint);
            SerializeRequestBody(typeWriter);
        }

        public override void Deserialize(ITypeReader typeReader)
        {
            base.Deserialize(typeReader);
            EndPoint = typeReader.ReadString();
            DeserializeRequestBody(typeReader);
        }
    }
    
    public interface ISessionRequest
    {
        Guid SessionId { get; set; }
    }

    public abstract class HttpSessionRequestBase : HttpSimpleRequestBase, ISessionRequest
    {
        public Guid SessionId { get; set; }

        protected HttpSessionRequestBase(string endpoint) : base(endpoint)
        {
        }

        public override void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(SessionId);
        }

        public override void Deserialize(ITypeReader typeReader)
        {
            SessionId = typeReader.ReadGuid();
        }
    }

    public abstract class HttpSimpleRequestBase : ISerializable
    {
        public string EndPoint { get; set; }

        protected HttpSimpleRequestBase(string endpoint)
        {
            EndPoint = endpoint;
        }

        public abstract void Serialize(ITypeWriter typeWriter);

        public abstract void Deserialize(ITypeReader typeReader);
    }
}