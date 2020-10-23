using System.Collections.Generic;
using System.Linq;

namespace Shaman.Contract.Routing
{
    public static class ServerInfoExtensions
    {
        public static IEnumerable<string> GetVersionIntersection(this ServerInfo firstServerInfo, ServerInfo secondServerInfo)
        {
            return firstServerInfo.ClientVersionList.Intersect(secondServerInfo.ClientVersionList);
        }
        
        public static bool AreVersionsIntersect(this ServerInfo firstServerInfo, ServerInfo secondServerInfo)
        {
            return firstServerInfo.GetVersionIntersection(secondServerInfo).Any();
        }
    }
}