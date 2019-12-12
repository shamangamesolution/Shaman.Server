using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Game.Providers;
using Shaman.ServerSharedUtilities;

namespace Shaman.Game
{
    public class GameModeControllerFactory : IGameModeControllerFactory
    {
        private readonly IGameBundle _gameBundle;
        private readonly IGameModeControllerFactory _bundledGameModeControllerFactory;

        public GameModeControllerFactory(IBundleInfoProvider bundleInfoProvider, IServerActualizer serverActualizer, IShamanComponents shamanComponents)
        {
            // in case of first time actualization
            serverActualizer.Actualize(0);
            _gameBundle = BundleHelper.LoadTypeFromBundle<IGameBundle>(bundleInfoProvider.GetBundleUri().Result);
            _gameBundle.OnInitialize(shamanComponents);
            _bundledGameModeControllerFactory = _gameBundle.GetGameModeControllerFactory();
        }

        public IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return _bundledGameModeControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
        }
    }
}