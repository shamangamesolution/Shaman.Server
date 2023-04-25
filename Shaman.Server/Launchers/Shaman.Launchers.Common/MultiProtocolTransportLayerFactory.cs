using System;
using Bro.WsShamanNetwork;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.LiteNetLibAdapter;

namespace Shaman.Launchers.Common;

public class MultiProtocolTransportLayerFactory : IServerTransportLayerFactory
{
    private readonly ITaskSchedulerFactory _taskSchedulerFactory;
    private readonly IShamanLogger _logger;

    public MultiProtocolTransportLayerFactory(ITaskSchedulerFactory taskSchedulerFactory, IShamanLogger logger)
    {
        _taskSchedulerFactory = taskSchedulerFactory;
        _logger = logger;
    }

    public ITransportLayer GetLayer(string protocol)
    {
        if (string.IsNullOrEmpty(protocol))
        {
            _logger.Info("No protocol specified, using default (udp)");
            return new LiteNetSock(_logger);
        }

        switch (protocol)
        {
            case "udp":
                _logger.Info("Using udp protocol");
                return new LiteNetSock(_logger);
            case "ws":
                _logger.Info("Using websocket protocol");
                return new WebSocketServerTransport(_taskSchedulerFactory.GetTaskScheduler(), _logger);
        }

        throw new NotImplementedException($"Protocol {protocol} not supported");
    }
}