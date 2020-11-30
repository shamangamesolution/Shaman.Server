using System;
using Shaman.Contract.Routing;

namespace Shaman.Launchers.Common
{
    public static class LauncherHelpers
    {
        public static string GetAssemblyName(ServerRole serverRole)
        {
            switch (serverRole)
            {
                case ServerRole.MatchMaker:
                    return "Shaman.MM";
                case ServerRole.GameServer:
                    return "Shaman.Game";
                default:
                    throw new ArgumentOutOfRangeException(nameof(serverRole), serverRole, null);
            }
        }
    }
}