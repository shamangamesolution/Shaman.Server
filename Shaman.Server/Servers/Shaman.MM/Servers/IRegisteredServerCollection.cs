using System.Collections.Generic;
using Shaman.Messages.MM;

namespace Shaman.MM.Servers
{
    public interface IRegisteredServerCollection
    {
        void RegisterServer(RegisteredServer server);
        void ActualizeServer(ServerIdentity id, Dictionary<ushort, int> peersCountPerPort);
        void UnregisterServer(ServerIdentity id);
        RegisteredServer GetLessLoadedServer();
        List<RegisteredServer> GetAll();
        void Clear();
        bool Contains(ServerIdentity id);
    }
}