using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shaman.ServiceBootstrap;

namespace Shaman.LocalBundleLauncher
{
    // public static class LocalBundleLauncher
    // {
    //     public static void Launch()
    //     {
    //         // var routerTask = LaunchRouterAndWaitUntilItStarts();
    //         var gameTask = LaunchGame();
    //         var mmTask = LaunchMM();
    //
    //         gameTask.Wait();
    //         mmTask.Wait();
    //         // routerTask.Wait();
    //     }
    //
    //     private static Task LaunchMM()
    //     {
    //         var mmConfig = LoadConfigFor("mm");
    //         var mmTask = Task.Factory.StartNew(() => Bootstrap.Launch<MM.Startup>(SourceType.MatchMaker, mmConfig));
    //         return mmTask;
    //     }
    //
    //     private static Task LaunchGame()
    //     {
    //         var gameConfig = LoadConfigFor("game");
    //         var gameTask =
    //             Task.Factory.StartNew(() => Bootstrap.Launch<Game.Startup>(SourceType.GameServer, gameConfig));
    //         return gameTask;
    //     }
    //
    //     private static Task LaunchRouterAndWaitUntilItStarts()
    //     {
    //         var routerConfig = LoadConfigFor("router");
    //         var routerTask =
    //             Task.Factory.StartNew(action: () => Bootstrap.Launch<Router.Startup>(SourceType.Router, routerConfig));
    //         EnsureRouterStarted(routerConfig, routerTask);
    //         return routerTask;
    //     }
    //
    //     private static void EnsureRouterStarted(IConfigurationRoot routerConfig, Task routerTask)
    //     {
    //         var routerHttpPort = int.Parse(routerConfig["BindToPortHttp"]);
    //         while (!CheckRouterIsAvailable(routerHttpPort) && routerTask.Status == TaskStatus.Running)
    //             Thread.Sleep(500);
    //     }
    //
    //     private static bool CheckRouterIsAvailable(int routerPort)
    //     {
    //         using (var cli = new WebClient())
    //         {
    //             try
    //             {
    //                 var data = cli.DownloadString($"http://localhost:{routerPort}/server/ping");
    //                 return data.Contains("\"resultCode\":1");
    //             }
    //             catch (WebException)
    //             {
    //                 return false;
    //             }
    //         }
    //     }
    //
    //     private static IConfigurationRoot LoadConfigFor(string configSuffix)
    //     {
    //         return new ConfigurationBuilder()
    //             .SetBasePath(Directory.GetCurrentDirectory())
    //             .AddJsonFile($"appsettings.{configSuffix}.json", optional: false)
    //             .AddJsonFile($"appsettings.{configSuffix}.Development.json", optional: true)
    //             .AddEnvironmentVariables()
    //             .Build();
    //     }
    // }
}