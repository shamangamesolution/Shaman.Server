namespace Shaman.Client.Peers
{
    public enum ShamanClientStatus
    {
        Offline,
        ConnectingMatchMaking,
        AuthorizingMatchMaking,
        JoiningMatchMaking,
        OnMatchMaking,
        LeavingMatchMaking,
        ConnectingGameServer,
        AuthorizingGameServer,
        JoiningRoom,
        InRoom,
        LeavingRoom,
        JoinFailed,
        CreateGameError,
        Disconnected,
        ErrorReceived
    }
}