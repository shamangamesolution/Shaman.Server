using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity.Router
{
    public class MatchMakerConfiguration : EntityBase    
    {
        public int Id { get; set; }
        public string Version { get; set; }    
        public string Name { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }
        public int BackendId { get; set; }
        public string BackendAddress { get; set; }
        public ushort BackendPort { get; set; }
        
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(Id);
            serializer.WriteString(Version);
            serializer.WriteString(Name);
            serializer.WriteString(Address);
            serializer.Write(Port);
            serializer.WriteString(BackendAddress);
            serializer.Write(BackendPort);
            serializer.Write(BackendId);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            Id = serializer.ReadInt();
            Version = serializer.ReadString();
            Name = serializer.ReadString();
            Address = serializer.ReadString();
            Port = serializer.ReadUShort();
            BackendAddress = serializer.ReadString();
            BackendPort = serializer.ReadUShort();
            BackendId = serializer.ReadInt();
        }
    }
}