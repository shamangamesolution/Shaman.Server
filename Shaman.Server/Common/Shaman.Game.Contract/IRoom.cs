using System;
using System.Collections.Generic;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;
using Shaman.Game.Contract.Stats;

namespace Shaman.Game.Contract
{
    
    public interface IRoom
    {
        Guid GetRoomId();
        bool PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties);
        void PeerLeft(Guid sessionId);
        void PeerDisconnected(Guid sessionId);
        void SendToAll(MessageBase message, params Guid[] exceptions);
        void AddToSendQueue(MessageBase message, Guid sessionId);
        void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId);
        void CleanUp();
        int GetPeerCount();
        RoomPlayer GetPlayer(Guid sessionId);
        DateTime GetCreatedOnDateTime();
        List<RoomPlayer> GetAllPlayers();
        void ConfirmedJoin(Guid sessionId);
        RoomStats GetStats();
        bool IsGameFinished();
        TimeSpan GetRoomTtl();
        void UpdateRoom(Dictionary<Guid, Dictionary<byte, object>> players);
        void AddToSendQueue(MessageData messageData, ushort opCode, Guid sessionId, bool isReliable, bool isOrdered);
    }
    
//    public class ClientInfo
//    {
//        string secret;
//    }
//    public interface ITaskScheduller
//    {
//        IDisposable ScheduleOnInterval(Action task, long firstMs, long periodMs);
//        IDisposable Schedule(Action task, long firstMs);
//    }
//    public interface IGameApplication
//    {
//        ITaskScheduller Scheduller {get}
//        CoreBufferPool BufferPool {get}
//    }
//    public class  Room
//    {
//        Room(string matchId, List<ClientInfo>, List<KeyValuePair<string, string>> metaInfo, IGameApplication app);
//        //Для резервирования мест после создания, берутся из клиент инфо, от мм
//        ReservePeer(string secret);
//        // надо чтобы не слать ивент мертвым
//        List<int> ActiveActors {get}
//        List<int> InactiveActors {get}
//        //ивенты приходят от пир менеджера
//        OnPeerJoined(IPeer, string secret);
//        OnPeerLeft(IPeer)
//        //призыв кикнуть данного пира 
//        DisconnectPeer(byte actorNr \ Peer);
//        //послать ивент если actorId == null то всем живым пирам в комнате, иначе конкретному
//        AddToSendQueue(byte eventCode, byte[] data, int? actorId, bool reliable, bool ordered);
//        OnDataReceived(byte eventCode, IPeer\actorNr, byte[] data)
//    }
}