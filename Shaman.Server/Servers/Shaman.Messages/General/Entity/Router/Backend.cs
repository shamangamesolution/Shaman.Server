using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity.Router
{
    public class Backend : EntityBase
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }

        public Backend()
        {
            
        }
        
        public Backend(int id, string address, ushort port)
        {
            Id = id;
            Address = address;
            Port = port;
        }
        
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(Id);
            serializer.Write(Address);
            serializer.Write(Port);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            Id = serializer.ReadInt();
            Address = serializer.ReadString();
            Port = serializer.ReadUShort();
        }
    }
}