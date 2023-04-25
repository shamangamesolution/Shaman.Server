using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;

namespace Bro.WsShamanNetwork;

public class WebSocketClientTransportLayerFactory : IClientTransportLayerFactory
{
    private readonly ITaskSchedulerFactory _taskSchedulerFactory;

    public WebSocketClientTransportLayerFactory(ITaskSchedulerFactory taskSchedulerFactory)
    {
        _taskSchedulerFactory = taskSchedulerFactory;
    }

    public ITransportLayer Create(IShamanLogger logger)
    {
        return new WebSocketClientTransport(_taskSchedulerFactory.GetTaskScheduler(), logger);
    }
}