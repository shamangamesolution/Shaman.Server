using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using Shaman.Contract.Routing;
using Shaman.DAL.SQL;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories;

namespace Shaman.Router.Tests
{
    [Ignore("Need DB instance")]
    public class ConfigurationRepositoryTests
    {
        private ConfigurationRepository _rep;

        [SetUp]
        public void Setup()
        {
            _rep = new ConfigurationRepository(
                new RouterSqlDalProvider(new OptionsWrapper<RouterConfiguration>(new RouterConfiguration
                {
                    DbConfig = GEtTestDbConfig()
                })));
        }

        [Test]
        public async Task CreateServerInfoTest()
        {
            var expected = new ServerInfo(new ServerIdentity("1.2.3.4", "2,3", ServerRole.GameServer), "name", "region",
                23, 1);
            Console.Out.WriteLine(JsonConvert.SerializeObject(expected, Formatting.Indented));

            var newId = await _rep.CreateServerInfo(expected);

            var actual = (await _rep.GetAllServerInfo()).Single(s => s.Id == newId);

            actual.Should().BeEquivalentTo(actual);
        }

        [Test]
        [Ignore("Manual")]
        public async Task GetBundlesInfoTest()
        {
            Console.WriteLine(JsonConvert.SerializeObject(await _rep.GetBundlesInfo(), Formatting.Indented));
        }

        [Test]
        public async Task GetServerIdTest()
        {
            var res = await _rep.GetServerId(new ServerIdentity("1.2.3.4", "2,3", ServerRole.GameServer));
            var id = res.Single();
            var actual = (await _rep.GetAllServerInfo()).Single(s => s.Id == id);
            actual.Address.Should().BeEquivalentTo("1.2.3.4");
        }

        [Test]
        public async Task UpdateServerInfoActualizedOnTest()
        {
            var res = await _rep.GetServerId(new ServerIdentity("1.2.3.4", "2,3", ServerRole.GameServer));
            var id = res.Single();
            await _rep.UpdateServerInfoActualizedOn(id, 12, 80, 81);
            var actual = (await _rep.GetAllServerInfo()).Single(s => s.Id == id);
            Console.Out.WriteLine(JsonConvert.SerializeObject(actual, Formatting.Indented));
            actual.PeerCount.Should().Be(12);
            actual.HttpPort.Should().Be(80);
            actual.HttpsPort.Should().Be(81);
        }

        private static SqlDbConfig GEtTestDbConfig()
        {
            return new SqlDbConfig
            {
                Database = "rw_router",
                Host = "localhost",
                User = "test",
                Password = "test",
                MaxPoolSize = 40
            };
        }
    }
}