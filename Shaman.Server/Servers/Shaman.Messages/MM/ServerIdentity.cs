using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class ServerIdentity : EntityBase
    {
        public string IpAddress { get; set; }
        public List<ushort> Ports { get; set; }

        private Guid _id { get; set; }
        
        public ServerIdentity(string ipAddress, List<ushort> ports)
        {
            IpAddress = ipAddress;
            Ports = ports;
            _id = Guid.NewGuid();
        }

        public ServerIdentity()
        {
            IpAddress = "";
            Ports = new List<ushort>();
        }

        public override string ToString()
        {
            var str = $"{IpAddress}:[";
            foreach (var port in Ports)
                str += port.ToString();
            str += "]";
            return str;
        }

        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteString(this.IpAddress);
            serializer.WriteInt(Ports.Count);
            foreach(var port in Ports)
                serializer.WriteUShort(port);
            serializer.WriteBytes(_id.ToByteArray());
                
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            this.IpAddress = serializer.ReadString();
            var count = serializer.ReadInt();
            this.Ports = new List<ushort>();
            for (var i = 0; i < count; i++)
                this.Ports.Add(serializer.ReadUShort());
            _id = new Guid(serializer.ReadBytes());
        }
        
        public class EqualityComparer : IEqualityComparer<ServerIdentity> {

            public bool Equals(ServerIdentity x, ServerIdentity y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;

                return x._id == y._id;
            }

            public int GetHashCode(ServerIdentity x)
            {
                return x._id.GetHashCode();
            }

        }
    }
}