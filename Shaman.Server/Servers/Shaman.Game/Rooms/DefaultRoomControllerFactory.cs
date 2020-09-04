using System;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;

namespace Shaman.Game.Rooms
{
    /// <summary>
    /// This implementation creates room controller factory using bundle passed through ctor
    /// </summary>
    public class DefaultRoomControllerFactory : IRoomControllerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IGameBundle _gameBundle;
        private readonly IRoomControllerFactory _bundledRoomControllerFactory;

        public DefaultRoomControllerFactory(IBundleLoader bundleLoader, IShamanComponents shamanComponents)
        {
            // var bundleUri = bundleInfoProvider.GetBundleUri().Result;
            // _gameBundle = BundleHelper.LoadTypeFromBundle<IGameBundle>(bundleUri, ((GameApplicationConfig)config).OverwriteDownloadedBundle);
            bundleLoader.LoadBundle();
            _gameBundle = bundleLoader.LoadTypeFromBundle<IGameBundle>();
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