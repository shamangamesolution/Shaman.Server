using Shaman.Contract.Routing;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Balancing.Messages
{
    public class ActualizeServerOnRouterRequest : HttpRequestBase
    {
        public ServerIdentity ServerIdentity { get; set; }
        public string Name { get; set; }
        public string Region {get;set;}
        public int PeersCount { get; set; }
        public ushort HttpPort { get; set; }
        public ushort HttpsPort { get; set; }
        public string State { get; set; }

        public ActualizeServerOnRouterRequest() : base(RouterEndpoints.ActualizeServer)
        {
            
        }
        
        public ActualizeServerOnRouterRequest(ServerIdentity serverIdentity, string name, string region, int peersCount,
            ushort httpPort = 0, ushort httpsPort = 0, string state = null) : this()
        {
            ServerIdentity = serverIdentity;
            Name = name;
            Region = region;
            PeersCount = peersCount;
            HttpPort = httpPort;
            HttpsPort = httpsPort;
            State = state;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ServerIdentity);
            typeWriter.Write(Name);
            typeWriter.Write(Region);
            typeWriter.Write(PeersCount);
            typeWriter.Write(HttpPort);
            typeWriter.Write(HttpsPort);
            typeWriter.Write(State);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ServerIdentity = typeReader.Read<ServerIdentity>();
            Name = typeReader.ReadString();
            Region = typeReader.ReadString();
            PeersCount = typeReader.ReadInt();
            HttpPort = typeReader.ReadUShort();
            HttpsPort = typeReader.ReadUShort();
            State = typeReader.ReadString();
        }
    }
}