using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Shaman.LocalBundleLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var routerConfig = LoadConfigFor("router");
            var gameConfig = LoadConfigFor("game");
            var mmConfig = LoadConfigFor("mm");

            var routerTask = Task.Factory.StartNew(action: () => { Shaman.Router.Program.Start(routerConfig); });

            var routerHttpPort = int.Parse(routerConfig["BindToPortHttp"]);
            while (!CheckRouterIsAvailable(routerHttpPort) && routerTask.Status == TaskStatus.Running)
                Thread.Sleep(500);

            var gameTask = Task.Factory.StartNew(() => Shaman.Game.Program.Start(gameConfig));
            var mmTask = Task.Factory.StartNew(() => Shaman.MM.Program.Start(mmConfig));

            gameTask.Wait();
            mmTask.Wait();
            routerTask.Wait();
        }

        private static bool CheckRouterIsAvailable(int routerPort)
        {
            using (var cli = new WebClient())
            {
                try
                {
                    var data = cli.DownloadString($"http://localhost:{routerPort}/server/ping");
                    return data.Contains("\"resultCode\":1");
                }
                catch (WebException)
                {
                    return false;
                }
            }
        }

        private static IConfigurationRoot LoadConfigFor(string configSuffix)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{configSuffix}.json", optional: false)
                .AddJsonFile($"appsettings.{configSuffix}.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}