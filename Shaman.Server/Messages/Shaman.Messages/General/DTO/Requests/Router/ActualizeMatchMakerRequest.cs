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

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Name);
            typeWriter.Write(IpAddress);
            typeWriter.Write(Port);
            typeWriter.Write(Secret);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            Name = typeReader.ReadString();
            IpAddress = typeReader.ReadString();
            Port = typeReader.ReadUShort();
            Secret = typeReader.ReadString();
        }
    }
}