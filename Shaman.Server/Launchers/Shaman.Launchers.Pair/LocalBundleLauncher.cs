using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.ServiceBootstrap;

namespace Shaman.LocalBundleLauncher
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
            var mmConfig = LoadConfigFor("mm");
            var mmTask = Task.Factory.StartNew(() => Bootstrap.Launch<Launchers.MM.Startup>(ServerRole.MatchMaker, mmConfig));
            return mmTask;
        }

        private static Task LaunchGame()
        {
            var gameConfig = LoadConfigFor("game");
            var gameTask =
                Task.Factory.StartNew(() => Bootstrap.Launch<Launchers.Game.Startup>(ServerRole.GameServer, gameConfig));
            return gameTask;
        }

        private static IConfigurationRoot LoadConfigFor(string configSuffix)
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"appsettings.common.json", optional: false)
                .AddJsonFile($"appsettings.common.{configSuffix}.json", optional: false)
                .AddJsonFile($"appsettings.{configSuffix}.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}