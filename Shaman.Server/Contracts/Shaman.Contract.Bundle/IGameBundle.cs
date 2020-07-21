namespace Shaman.Game.Contract
{
    public interface IGameBundle
    {
        IGameModeControllerFactory GetGameModeControllerFactory();
        void OnInitialize(IShamanComponents shamanComponents);
    }
}