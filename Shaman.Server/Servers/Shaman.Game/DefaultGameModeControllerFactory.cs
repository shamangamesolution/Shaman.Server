using System;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.ServerSharedUtilities;
using Shaman.ServerSharedUtilities.Bundling;

namespace Shaman.Game
{
    public class DefaultGameModeControllerFactory : IGameModeControllerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IGameBundle _gameBundle;
        private readonly IGameModeControllerFactory _bundledGameModeControllerFactory;

        public DefaultGameModeControllerFactory(IBundleInfoProvider bundleInfoProvider,
            IServerActualizer serverActualizer, IShamanComponents shamanComponents, IApplicationConfig config)
        {
            // in case of first time actualization
            serverActualizer.Actualize(0);
            var bundleUri = bundleInfoProvider.GetBundleUri().Result;
            _gameBundle = BundleHelper.LoadTypeFromBundle<IGameBundle>(bundleUri, ((GameApplicationConfig)config).OverwriteDownloadedBundle);
            _gameBundle.OnInitialize(shamanComponents);
            _bundledGameModeControllerFactory = _gameBundle.GetGameModeControllerFactory();
            if (_bundledGameModeControllerFactory == null)
            {
                throw new NullReferenceException("Game bundle returned null factory");
            }
        }

        public IGameModeController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return _bundledGameModeControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
        }
    }
}