using Shaman.Contract.Bundle;

namespace Shaman.Game.Rooms
{
    public interface IBundleRoomControllerRegistry
    {
        void RegisterBundleRoomController(IRoomControllerFactory roomControllerFactory);
    }
}