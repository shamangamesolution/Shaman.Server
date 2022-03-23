using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Common.Logging;

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
        private readonly IShamanLogger _logger;
        
        public ProtectionManager(IConnectDdosProtection ddosProtection, IProtectionManagerConfig config, IShamanLogger logger)
        {
            _ddosProtection = ddosProtection;
            _config = config;
            _logger = logger;
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
            try
            {
                if (_config.IsConnectionDdosProtectionOn)
                    return _ddosProtection.IsBanned(endPoint);
            }
            catch (Exception e)
            {
                //if something go wrong - we return false
                _logger.Error($"IsBanned error: {e}");
                return false;
            }
            
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