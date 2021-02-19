using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Common.Server.Protection;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Tests.Configuration;

namespace Shaman.Tests
{
    [TestFixture]
    public class ConnectionDdosProtectorTests
    {
        [Test]
        public async Task BaseTest()
        {
            var connectionsCount = 10;
            var ip = IPAddress.Loopback;
            var logger = new ConsoleLogger();
            var protector = new ConnectDdosProtection(
                new ConnectionDdosProtectionConfig(connectionsCount, 100, 200, 100)
                {
                    IsConnectionDdosProtectionOn = true
                },
                new TaskSchedulerFactory(logger),
                logger
            );
            var emptyTask = new Task(() => {});
            Assert.IsFalse(protector.IsBanned(new IPEndPoint(ip, 1)));
            for (var i = 0; i < connectionsCount; i++)
            {
                protector.OnPeerConnected(new IPEndPoint(ip, 1));
            }
            protector.Start();
            emptyTask.Wait(101);
            Assert.IsTrue(protector.IsBanned(new IPEndPoint(ip, 1)));
            emptyTask.Wait(401);
            Assert.IsFalse(protector.IsBanned(new IPEndPoint(ip, 1)));
            protector.Stop();
        }
    }
}