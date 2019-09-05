using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class ActualizeMatchMakerRequest : RequestBase
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public ushort Port { get; set; }

        public string Secret { get; set; }
        public GameProject GameProject { get; set; }
        public ActualizeMatchMakerRequest()
            :base(CustomOperationCode.ActualizeMatchmaker, BackEndEndpoints.ActualizeMatchMaker)
        {
            
        }
        
        public ActualizeMatchMakerRequest(GameProject gameProject, string name, string ipAddress, ushort port, string secret) : this()
        {
            Name = name;
            IpAddress = ipAddress;
            Port = port;
            Secret = secret;
            GameProject = gameProject;
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.Write(Name);
            serializer.Write(IpAddress);
            serializer.Write(Port);
            serializer.Write(Secret);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            Name = serializer.ReadString();
            IpAddress = serializer.ReadString();
            Port = serializer.ReadUShort();
            Secret = serializer.ReadString();
        }
    }
}