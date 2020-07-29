namespace Shaman.Contract.Bundle
{
    public interface IGameBundle
    {
        IRoomControllerFactory GetRoomControllerFactory();
        void OnInitialize(IShamanComponents shamanComponents);
    }
}