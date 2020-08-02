using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Messages
{
    public class ServerIdentity : EntityBase, IEquatable<ServerIdentity>
    {
        public string Address { get; set; }
        public List<ushort> Ports { get; set; }
        private Guid _id { get; set; }
        public string PortsString { get; set; }
        public ServerRole ServerRole { get; set; }

        #region helpers
        private static string GetAsPortString(IEnumerable<ushort> list, char separator = ',')
        {
            StringBuilder stringBuilder = new StringBuilder();
            var isFirst = true;
            foreach (var item in list)
            {
                if (!isFirst)
                    stringBuilder.Append(separator);
                isFirst = false;
                stringBuilder.Append(item.ToString());
            }
            return stringBuilder.ToString();
        }
        private static IEnumerable<ushort> GetAsUshortList(string sourceString,  char separator = ',')
        {
            var ports = new List<ushort>();
            var portsSplitted = sourceString.Split(separator);
            foreach (var port in portsSplitted)
            {
                if (!ushort.TryParse(port, out var numberPort))
                    throw new Exception($"Can not parse string as List<ushort> {sourceString}");
                ports.Add(numberPort);
            }

            return ports;
        }
        #endregion
        
        public ServerIdentity(string address, List<ushort> ports, ServerRole serverRole)
        {
            Address = address;
            Ports = ports;
            ServerRole = serverRole;
            PortsString = GetAsPortString(Ports);
            _id = Guid.NewGuid();
        }
        
        public ServerIdentity(string address, string ports, ServerRole serverRole)
        {
            Address = address;
            PortsString = ports;
            ServerRole = serverRole;
            Ports = GetAsUshortList(PortsString).ToList();
            _id = Guid.NewGuid();
        }

        public ServerIdentity()
        {
            Address = "";
            PortsString = "";
            Ports = new List<ushort>();
        }

        public override string ToString()
        {
            var str = $"{ServerRole}://{Address}:[";
            foreach (var port in Ports)
                str += port.ToString();
            str += "]";
            return str;
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write((byte)this.ServerRole);
            typeWriter.Write(this.Address);
            typeWriter.Write(Ports.Count);
            foreach(var port in Ports)
                typeWriter.Write(port);
            typeWriter.Write(_id);
            typeWriter.Write(this.PortsString);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            this.ServerRole = (ServerRole) typeReader.ReadByte();
            this.Address = typeReader.ReadString();
            var count = typeReader.ReadInt();
            this.Ports = new List<ushort>();
            for (var i = 0; i < count; i++)
                this.Ports.Add(typeReader.ReadUShort());
            _id = typeReader.ReadGuid();
            PortsString = typeReader.ReadString();
        }
        
        public class Equali2tyComparer : IEqualityComparer<ServerIdentity> {

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

        public bool Equals(ServerIdentity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var a = Ports.All(other.Ports.Contains) && Ports.Count == other.Ports.Count;
            return string.Equals(Address, other.Address) && a &&
                   string.Equals(PortsString, other.PortsString) && ServerRole == other.ServerRole;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerIdentity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Address != null ? Address.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Ports != null ? Ports.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _id.GetHashCode();
                hashCode = (hashCode * 397) ^ (PortsString != null ? PortsString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) ServerRole;
                return hashCode;
            }
        }
    }
}