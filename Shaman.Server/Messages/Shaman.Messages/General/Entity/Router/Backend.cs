using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Messages.General.Entity.Router
{
    public class Backend : EntityBase
    {
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
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Address);
            typeWriter.Write(Port);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Address = typeReader.ReadString();
            Port = typeReader.ReadUShort();
        }
    }
}