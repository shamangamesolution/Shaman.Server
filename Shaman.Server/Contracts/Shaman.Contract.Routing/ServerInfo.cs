using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Contract.Routing
{
    public class ServerInfo : EntityBase
    {
        public string Address { get; set; } = "";
        public string Ports { get; set; } = "";
        public ushort HttpPort { get; set; }
        public ushort HttpsPort { get; set; }
        public ServerRole ServerRole { get; set; }
        public string Name { get; set; } = "";
        public string Region { get; set; } = "";
        public string ClientVersion { get; set; } = "";
        public TimeSpan ActualizedGap { get; set; }
        public bool IsApproved { get; set; }
        public int PeerCount { get; set; }
        
        private ServerIdentity _identity;
        private IEnumerable<string> _clientVersionList;
        
        public ServerIdentity Identity
        {
            get
            {
                if (_identity == null)
                    _identity = new ServerIdentity(Address, Ports, ServerRole);
                return _identity;
            }
        }

        public IEnumerable<string> ClientVersionList
        {
            get
            {
                if (_clientVersionList == null || !_clientVersionList.Any())
                {
                    var arr = ClientVersion.Split(',');
                    for (var i = 0; i < arr.Length; i++)
                        arr[i] = arr[i].Trim();
                    _clientVersionList = arr.Where(i => !string.IsNullOrWhiteSpace(i));
                }

                return _clientVersionList;
            }
        }

        public ServerInfo()
        {
            
        }
        
        public bool IsActual(int actualTimeoutMs)
        {
            return IsApproved && ActualizedGap.TotalMilliseconds <= actualTimeoutMs;
        }
        
        public ServerInfo(ServerIdentity identity, string name, string region, ushort httpPort, ushort httpsPort = 0)
        {
            Address = identity.Address;
            Ports = identity.PortsString;
            ServerRole = identity.ServerRole;
            Name = name;
            Region = region;
            HttpPort = httpPort;
            HttpsPort = httpsPort;
        }

        public ushort GetLessLoadedPort()
        {
            return Identity.Ports.FirstOrDefault();
        }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Address);
            typeWriter.Write(Ports);
            typeWriter.Write((byte)ServerRole);
            typeWriter.Write(Name);
            typeWriter.Write(Region);
            typeWriter.Write(ClientVersion);
            typeWriter.Write(ActualizedGap);
            typeWriter.Write(IsApproved);
            typeWriter.Write(PeerCount);
            typeWriter.Write(HttpPort);
            typeWriter.Write(HttpsPort);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Address = typeReader.ReadString();
            Ports = typeReader.ReadString();
            ServerRole = (ServerRole) typeReader.ReadByte();
            Name = typeReader.ReadString();
            Region = typeReader.ReadString();
            ClientVersion = typeReader.ReadString();
            ActualizedGap = typeReader.ReadTimeSpan();
            IsApproved = typeReader.ReadBool();
            PeerCount = typeReader.ReadInt();
            HttpPort = typeReader.ReadUShort();
            HttpsPort = typeReader.ReadUShort();
        }
    }
}