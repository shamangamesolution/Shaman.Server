namespace Shaman.Contract.Bundle
{
    public interface IGameBundle
    {
        IRoomControllerFactory GetRoomControllerFactory();
        void Initialize(IShamanComponents shamanComponents);
        void OnStart();
    }
}