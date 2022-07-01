using Shaman.Contract.Routing;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Balancing.Messages
{
    public class GetStateRequest : HttpRequestBase
    {
        public ServerIdentity ServerIdentity { get; set; }
        
        public GetStateRequest(ServerIdentity serverIdentity) : this()
        {
            ServerIdentity = serverIdentity;
        }
        
        public GetStateRequest() 
            : base(RouterEndpoints.GetState)
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ServerIdentity);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ServerIdentity = typeReader.Read<ServerIdentity>();
        }
    }
    
    public class GetStateResponse : HttpResponseBase
    {
        public string State { get; set; }
        
        public GetStateResponse()
        {
        }
        
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(State);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            State = typeReader.ReadString();
        }
    }
}