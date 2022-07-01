using Shaman.Contract.Routing;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Balancing.Messages
{
    public class SaveStateRequest : HttpRequestBase
    {
        public ServerIdentity ServerIdentity { get; set; }
        public string State { get; set; }
        
        public SaveStateRequest(ServerIdentity serverIdentity, string state) : this()
        {
            ServerIdentity = serverIdentity;
            State = state;
        }
        
        public SaveStateRequest() 
            : base(RouterEndpoints.SaveState)
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ServerIdentity);
            typeWriter.Write(State);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ServerIdentity = typeReader.Read<ServerIdentity>();
            State = typeReader.ReadString();
        }
    }
    
    public class SaveStateResponse : HttpResponseBase
    {
        public SaveStateResponse() : base()
        {
        }
        
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
        }
    }
}