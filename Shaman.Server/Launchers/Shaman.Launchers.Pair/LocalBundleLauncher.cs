using System.Threading.Tasks;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Pair
{
    public static class LocalBundleLauncher
    {
        public static void Launch()
        {
            var gameTask = LaunchGame();
            var mmTask = LaunchMm();

            gameTask.Wait();
            mmTask.Wait();
        }

        private static Task LaunchMm()
        {
            var mmTask = Task.Factory.StartNew(() => Bootstrap.LaunchWithCommonAndRoleConfig<Launchers.MM.Startup>(ServerRole.MatchMaker.ToString()));
            return mmTask;
        }

        private static Task LaunchGame()
        {
            var gameTask =
                Task.Factory.StartNew(() => Bootstrap.LaunchWithCommonAndRoleConfig<Launchers.Game.Startup>(ServerRole.GameServer.ToString()));
            return gameTask;
        }
    }
}