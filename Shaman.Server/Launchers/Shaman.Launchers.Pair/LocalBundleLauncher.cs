using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Pair
{
    public static class LocalBundleLauncher
    {
        public static void Launch()
        {
            var gameTask = LaunchGame();
            var mmTask = LaunchMM();

            gameTask.Wait();
            mmTask.Wait();
        }

        private static Task LaunchMM()
        {
            var mmTask = Task.Factory.StartNew(() => Bootstrap.LaunchWithCommonAndRoleConfig<Launchers.MM.Startup>(ServerRole.MatchMaker));
            return mmTask;
        }

        private static Task LaunchGame()
        {
            var gameTask =
                Task.Factory.StartNew(() => Bootstrap.LaunchWithCommonAndRoleConfig<Launchers.Game.Startup>(ServerRole.GameServer));
            return gameTask;
        }
    }
}