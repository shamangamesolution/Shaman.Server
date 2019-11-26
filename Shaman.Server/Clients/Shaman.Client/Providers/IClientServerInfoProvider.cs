using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Client.Providers
{
    public interface IClientServerInfoProvider
    {
        Task GetRoutes(string routerUrl, string clientVersion, Action<List<Route>> callback);
    }
}