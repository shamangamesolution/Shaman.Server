namespace Shaman.Common.Server.Peers
{
    public enum DisconnectReason : byte
    {
        JustBecause = 1,
        ServerShutDown = 2,
        RoomCleanup = 3,
        ErrorGettingRoomParameters = 4
    }
}