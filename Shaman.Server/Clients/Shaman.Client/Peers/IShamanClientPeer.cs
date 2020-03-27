using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Utils.Messages;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.RoomFlow;

namespace Shaman.Client.Peers
{
    public interface IShamanClientPeer
    {
        Action<string> OnDisconnected { get; set; }
        Action<string> OnDisconnectedFromMmServer { get; set; }
        Action<string> OnDisconnectedFromGameServer { get; set; }

        void Connect(string address, ushort port);
        Task<JoinInfo> JoinGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties);

        Task<JoinInfo> DirectConnectToGameServer(string gameServerAddress, ushort gameServerPort, Guid sessionId,  Guid roomId, Dictionary<byte, object> joinGameProperties);
        Task<TResponse> SendRequest<TResponse>(RequestBase request) where TResponse : ResponseBase, new();

        Guid RegisterOperationHandler<T>(Action<T> handler,
            bool callOnce = false) where T : MessageBase, new();

        void UnregisterOperationHandler(Guid id);

        Task<T> SendWebRequest<T>(string url, HttpRequestBase request)
            where T : HttpResponseBase, new();

        void SendEvent(MessageBase eve);
        void Disconnect();
        void ProcessMessages();
        ShamanClientStatus GetStatus();
        Task<int> Ping(Route route, int timeoutMs = 500);
    }
}