using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;

namespace Shaman.Common.Server.Protection
{
    public interface IProtectionManager
    {
        void PeerConnected(IPEndPoint endPoint);
        void OnReceivePacket(IPEndPoint endPoint);
        bool IsBanned(IPEndPoint endPoint);
        void Start();
        void Stop();
    }
    
    public class ProtectionManager : IProtectionManager
    {
        private readonly IConnectDdosProtection _ddosProtection;
        private readonly IProtectionManagerConfig _config;
        
        public ProtectionManager(IConnectDdosProtection ddosProtection, IProtectionManagerConfig config)
        {
            _ddosProtection = ddosProtection;
            _config = config;
        }

        public void PeerConnected(IPEndPoint endPoint)
        {
            if (_config.IsConnectionDdosProtectionOn)
                _ddosProtection.OnPeerConnected(endPoint);
        }

        public void OnReceivePacket(IPEndPoint endPoint)
        {
            //nothing here
        }

        public bool IsBanned(IPEndPoint endPoint)
        {
            if (_config.IsConnectionDdosProtectionOn)
                return _ddosProtection.IsBanned(endPoint);

            return false;
        }

        public void Start()
        {
            if (_config.IsConnectionDdosProtectionOn)
                _ddosProtection.Start();
        }

        public void Stop()
        {
            if (_config.IsConnectionDdosProtectionOn)
                _ddosProtection.Stop();
        }
    }
}