using System.Net;
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

        public ProtectionManager(IConnectDdosProtection ddosProtection)
        {
            _ddosProtection = ddosProtection;
        }

        public void PeerConnected(IPEndPoint endPoint)
        {
            _ddosProtection.OnPeerConnected(endPoint);
        }

        public void OnReceivePacket(IPEndPoint endPoint)
        {
            //nothing here
        }

        public bool IsBanned(IPEndPoint endPoint)
        {
            return _ddosProtection.IsBanned(endPoint);
        }

        public void Start()
        {
            _ddosProtection.Start();
        }

        public void Stop()
        {
            _ddosProtection.Stop();
        }
    }
}