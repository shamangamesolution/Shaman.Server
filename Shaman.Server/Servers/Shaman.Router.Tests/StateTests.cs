using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.DAL.SQL;
using Shaman.Router.Config;
using Shaman.Router.Data.Providers;
using Shaman.Router.Data.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;

namespace Shaman.Router.Tests;

[Ignore("Need DB instance")]
public class StateTests
{
    private IStatesManager _statesManager;
    private IShamanLogger _logger;
    private IRouterServerInfoProvider _serverInfoProvider;
    private IConfigurationRepository _configurationRepository;
    private ITaskSchedulerFactory _taskSchedulerFactory;

    private static SqlDbConfig GetTestDbConfig()
    {
        return new SqlDbConfig
        {
            Database = "ap_router",
            Host = "localhost",
            User = "test",
            Password = "test",
            MaxPoolSize = 40
        };
    }
    
    [SetUp]
    public void Setup()
    {
        _logger = new ConsoleLogger();
        _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
        var config = new OptionsWrapper<RouterConfiguration>(new RouterConfiguration
        {
            DbConfig = GetTestDbConfig(),
            ServerInfoListUpdateIntervalMs = 1000
        });
        var dalProvider = new RouterSqlDalProvider(config);

        _configurationRepository = new ConfigurationRepository(dalProvider);

        _serverInfoProvider =
            new RouterServerInfoProvider(_configurationRepository, _taskSchedulerFactory, config, _logger);
        _statesManager =
            new StatesManager(dalProvider, _serverInfoProvider, _logger, _taskSchedulerFactory, config);
    }
    
    [Test]
    public async Task Tests()
    {
        _serverInfoProvider.Start();
        _statesManager.Start();
        await Task.Delay(1000);
        var entityDictionary = _serverInfoProvider.GetAllServers();
        Assert.IsNotEmpty(entityDictionary);
        var serverIdentity = new ServerIdentity(){Address = "127.0.0.1", Ports = new List<ushort> {23452},  PortsString = "23452", ServerRole = ServerRole.GameServer};
        _serverInfoProvider.GetAllBundles();
        var anObject = await _statesManager.GetState(serverIdentity);
        Assert.IsNull(anObject);
        await _statesManager.SaveState(serverIdentity, "teststate");
        await Task.Delay(1000);
        anObject = await _statesManager.GetState(serverIdentity);
        Assert.IsNotNull(anObject);
        Assert.AreEqual("teststate", anObject);
        await _statesManager.SaveState(serverIdentity, "teststate2");
        anObject = await _statesManager.GetState(serverIdentity);
        Assert.IsNotNull(anObject);
        Assert.AreEqual("teststate2", anObject);
    }
}