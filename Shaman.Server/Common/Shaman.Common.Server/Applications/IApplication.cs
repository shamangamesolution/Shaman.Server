using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Server.Applications
{
    public interface IApplication 
    {
        void Initialize(IShamanLogger logger, IApplicationConfig config, ISerializerFactory serializerFactory, ISocketFactory socketFactory, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender);
        void Start();
        void ShutDown();
    }
}