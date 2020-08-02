using System;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Game.Configuration;
using Shaman.ServerSharedUtilities;
using Shaman.ServerSharedUtilities.Bundling;

namespace Shaman.Game
{
    public class DefaultRoomControllerFactory : IRoomControllerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IGameBundle _gameBundle;
        private readonly IRoomControllerFactory _bundledRoomControllerFactory;

        public DefaultRoomControllerFactory(IBundleInfoProvider bundleInfoProvider,
            IServerActualizer serverActualizer, IShamanComponents shamanComponents, IApplicationConfig config)
        {
            // in case of first time actualization
            serverActualizer.Actualize(0);
            var bundleUri = bundleInfoProvider.GetBundleUri().Result;
            _gameBundle = BundleHelper.LoadTypeFromBundle<IGameBundle>(bundleUri, ((GameApplicationConfig)config).OverwriteDownloadedBundle);
            _gameBundle.OnInitialize(shamanComponents);
            _bundledRoomControllerFactory = _gameBundle.GetRoomControllerFactory();
            if (_bundledRoomControllerFactory == null)
            {
                throw new NullReferenceException("Game bundle returned null factory");
            }
        }

        public IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return _bundledRoomControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
        }
    }
}