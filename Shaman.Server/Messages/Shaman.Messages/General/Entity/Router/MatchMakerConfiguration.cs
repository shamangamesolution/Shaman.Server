using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Messages.General.Entity.Router
{
    public class MatchMakerConfiguration : EntityBase    
    {
        public string Version { get; set; }    
        public string Name { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }
        public int BackendId { get; set; }
        public string BackendAddress { get; set; }
        public ushort BackendPort { get; set; }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Version);
            typeWriter.Write(Name);
            typeWriter.Write(Address);
            typeWriter.Write(Port);
            typeWriter.Write(BackendAddress);
            typeWriter.Write(BackendPort);
            typeWriter.Write(BackendId);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Version = typeReader.ReadString();
            Name = typeReader.ReadString();
            Address = typeReader.ReadString();
            Port = typeReader.ReadUShort();
            BackendAddress = typeReader.ReadString();
            BackendPort = typeReader.ReadUShort();
            BackendId = typeReader.ReadInt();
        }
    }
}