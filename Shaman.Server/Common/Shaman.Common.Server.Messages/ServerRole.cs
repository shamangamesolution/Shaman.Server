namespace Shaman.Common.Server.Messages
{
    public enum ServerRole : byte
    {
        BackEnd = 1,
        MatchMaker = 2,
        GameServer = 3,
        Router = 4
    }
}