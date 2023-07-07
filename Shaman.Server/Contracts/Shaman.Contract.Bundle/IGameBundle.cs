
namespace Shaman.Contract.Bundle
{
    public interface IGameBundle
    {
        IRoomControllerFactory GetRoomControllerFactory();
        void Initialize(IShamanComponents shamanComponents);
        IGameMetrics GetMetrics(IShamanComponents shamanComponents);
        void OnStart();
    }
}