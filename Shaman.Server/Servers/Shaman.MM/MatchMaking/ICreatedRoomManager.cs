namespace Shaman.MM.MatchMaking
{
    public interface ICreatedRoomManager
    {
        void AddCreatedRoom(CreatedRoom createdRoom);
        CreatedRoom GetRoomForPlayers(int playersCount);
        int GetCreatedRoomsCount();
        void Start();
        void Stop();
    }
}