using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shaman.Client.Providers
{
    public interface IClientServerInfoProvider
    {
        Task GetRoutes(string routerUrl, string clientVersion, Action<List<Route>> callback);
        Task<List<Route>> GetRoutes(string routerUrl, string clientVersion);
    }
}