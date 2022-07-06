using System;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Routing.Balancing.Messages;

namespace Shaman.Launchers.Common.Balancing
{
    public interface IServerStateProvider
    {
        Task<string> GetState();
    }

    public class ServerStateProvider : IServerStateProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IBalancingBundleInfoProviderConfig _config;

        public ServerStateProvider(IRequestSender requestSender, IBalancingBundleInfoProviderConfig config,
            IShamanLogger logger)
        {
            _requestSender = requestSender;
            _logger = logger;
            _config = config;
        }

        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.PublicName,
                _config.Ports, _config.Role);
        }


        public async Task<string> GetState()
        {
            var myIdentity = GetServerIdentity();
            var response = await _requestSender.SendRequest<GetStateResponse>(_config.RouterUrl,
                new GetStateRequest {ServerIdentity = myIdentity});

            if (!response.Success)
                throw new Exception(
                    $"Server state request failed for {myIdentity}, requested from '{_config.RouterUrl}': {response.Message}");

            _logger.Error($"Server state received for '{myIdentity}' : {response.State}");
            return response.State;
        }
    }
}