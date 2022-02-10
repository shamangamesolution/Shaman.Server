using Shaman.Contract.Bundle;

namespace Shaman.Game.Rooms
{
    public interface IBundledRoomControllerFactory: IRoomControllerFactory
    {
        void RegisterBundleRoomController(IRoomControllerFactory roomControllerFactory);
    }
}