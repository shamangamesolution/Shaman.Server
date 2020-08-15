using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shaman.Serialization;

namespace Shaman.Router.Messages
{
    public class ServerIdentity : ISerializable
    {
        public string Address { get; set; }
        public List<ushort> Ports { get; set; }
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
        }
        
        public ServerIdentity(string address, string ports, ServerRole serverRole)
        {
            Address = address;
            PortsString = ports;
            ServerRole = serverRole;
            Ports = GetAsUshortList(PortsString).ToList();
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

        public bool Equals(ServerIdentity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var a = Ports.All(other.Ports.Contains) && Ports.Count == other.Ports.Count;
            return string.Equals(Address, other.Address) && a &&
                   string.Equals(PortsString, other.PortsString) && ServerRole == other.ServerRole;
        }

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write((byte)this.ServerRole);
            typeWriter.Write(this.Address);
            typeWriter.Write(Ports.Count);
            foreach(var port in Ports)
                typeWriter.Write(port);
            typeWriter.Write(this.PortsString);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            this.ServerRole = (ServerRole) typeReader.ReadByte();
            this.Address = typeReader.ReadString();
            var count = typeReader.ReadInt();
            this.Ports = new List<ushort>();
            for (var i = 0; i < count; i++)
                this.Ports.Add(typeReader.ReadUShort());
            PortsString = typeReader.ReadString();
        }
    }
}