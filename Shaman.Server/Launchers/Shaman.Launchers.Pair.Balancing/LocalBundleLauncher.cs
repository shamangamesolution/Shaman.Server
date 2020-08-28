using System.Threading.Tasks;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Pair.Balancing
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
            var mmTask = Task.Factory.StartNew(() => Bootstrap.LaunchWithCommonAndRoleConfig<Launchers.MM.Balancing.Startup>(ServerRole.MatchMaker));
            return mmTask;
        }

        private static Task LaunchGame()
        {
            var gameTask =
                Task.Factory.StartNew(() => Bootstrap.LaunchWithCommonAndRoleConfig<Launchers.Game.Balancing.Startup>(ServerRole.GameServer));
            return gameTask;
        }
    }
}