using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;

namespace Bro.WsShamanNetwork;

public class WebSocketClientTransportLayerFactory : IClientTransportLayerFactory
{
    private readonly ITaskSchedulerFactory _taskSchedulerFactory;
    private readonly TimeSpan _keepAliveTimeout;

    public WebSocketClientTransportLayerFactory(ITaskSchedulerFactory taskSchedulerFactory, TimeSpan keepAliveTimeout)
    {
        _taskSchedulerFactory = taskSchedulerFactory;
        _keepAliveTimeout = keepAliveTimeout;
    }

    public ITransportLayer Create(IShamanLogger logger)
    {
        return new WebSocketClientTransport(_taskSchedulerFactory.GetTaskScheduler(), logger, _keepAliveTimeout);
    }
}