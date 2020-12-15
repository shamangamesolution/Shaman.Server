namespace Shaman.LiteNetLibAdapter
{
    public enum ServerDisconnectReason : byte
    {
        JustBecause = 1,
        ServerShutDown = 2,
        RoomCleanup = 3,
        ErrorGettingRoomParameters = 4,
        KickedByServer = 5
    }
}