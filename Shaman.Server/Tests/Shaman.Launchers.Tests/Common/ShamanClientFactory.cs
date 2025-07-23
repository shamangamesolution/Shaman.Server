using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.LiteNetLibAdapter;
using Shaman.Serialization;
using Shaman.TestTools.ClientPeers;

namespace Shaman.Launchers.Tests.Common;

public class ShamanClientFactory
{
    private IShamanLogger _logger;
    private ITaskSchedulerFactory _taskSchedulerFactory;
    private ISerializer _serializer;
    private IRequestSender _requestSender;
    private IShamanClientPeerListener _clientPeerListener;
    private IShamanClientPeerConfig _clientPeerConfig;

    public ShamanClientFactory()
    {
        _logger = new ConsoleLogger("C", LogLevel.Error | LogLevel.Info);
        _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
        _serializer = new BinarySerializer();
        _requestSender = new TestClientHttpSender(_logger, _serializer);
        _clientPeerListener = new TestClientPeerListener(_logger);
        _clientPeerConfig = new ClientPeerConfig();
    }
        
    public IShamanClientPeer<byte> GetClient()
    {
        return new ShamanClientPeer<byte>(_logger, _taskSchedulerFactory, _serializer, _requestSender, _clientPeerListener, _clientPeerConfig, new LiteNetClientTransportLayerFactory(), new ByteOpCodeExtractor());
    }
    
    public IShamanClientPeer GetClient(IClientTransportLayerFactory transportLayerFactory)
    {
        return new ShamanClientPeer(_logger, _taskSchedulerFactory, _serializer, _requestSender, _clientPeerListener, _clientPeerConfig, transportLayerFactory);
    }
    
    public IShamanClientPeer<byte> GetClient(IShamanClientPeerListener peerListener)
    {
        return new ShamanClientPeer<byte>(_logger, _taskSchedulerFactory, _serializer, _requestSender, peerListener, _clientPeerConfig, new LiteNetClientTransportLayerFactory(), new ByteOpCodeExtractor());
    }
}