using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories;

namespace Shaman.Router.Tests
{
    public class ConfigurationRepositoryTests
    {
        [Test]
        public async Task Test1()
        {
            var options = new OptionsWrapper<RouterConfiguration>(new RouterConfiguration
            {
                DbServer = "localhost", DbName = "db_router", DbUser = "test", DbPassword = "test",
                DbMaxPoolSize = 4000
            });
            var repository = new ConfigurationRepository(options, new ConsoleLogger());

            var bundles = await repository.GetBundlesInfo();
            Console.Out.WriteLine("bundles = {0}", JsonConvert.SerializeObject(bundles));
            bundles.Count().Should().Be(2);
        }
    }
}