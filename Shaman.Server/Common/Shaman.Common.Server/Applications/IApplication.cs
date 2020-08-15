using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Server.Applications
{
    public interface IApplication 
    {
        void Start();
        void ShutDown();
    }
}