namespace Shaman.Contract.Bundle
{
    public interface IGameBundle
    {
        IGameModeControllerFactory GetGameModeControllerFactory();
        void OnInitialize(IShamanComponents shamanComponents);
    }
}