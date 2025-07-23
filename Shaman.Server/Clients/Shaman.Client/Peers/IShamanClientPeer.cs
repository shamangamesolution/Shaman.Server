using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Udp.Sockets;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client.Peers
{
    public interface IShamanClientPeer<TOpCode>
    {
        Action<IDisconnectInfo> OnDisconnected { get; set; }
        Action<IDisconnectInfo> OnDisconnectedFromMmServer { get; set; }
        Action<IDisconnectInfo> OnDisconnectedFromGameServer { get; set; }

        void Connect(string address, ushort port);

        Task<JoinInfo> DirectConnectToGameServerToRandomRoom(string gameServerAddress, ushort gameServerPort,
            Guid sessionId, Dictionary<byte, object> roomProperties, Dictionary<byte, object> joinGameProperties);

        Task<JoinInfo> DirectConnectToGameServer(string gameServerAddress, ushort gameServerPort, Guid sessionId,  Guid roomId, Dictionary<byte, object> joinGameProperties);

        Task<TResponse> SendRequest<TResponse>(IOperationCodeProvider<TOpCode> request)
            where TResponse : IOperationCodeProvider<TOpCode>, new();

        Guid RegisterOperationHandler<T>(Action<T, Exception> handler,
            bool callOnce = false) where T : IOperationCodeProvider<TOpCode>, new();

        void UnregisterOperationHandler(Guid id);

        Task<T> SendWebRequest<T>(string url, HttpRequestBase request)
            where T : HttpResponseBase, new();

        void SendEvent<TMessage>(TMessage eve, IUdpOptions udpOptions = null) where TMessage : IOperationCodeProvider<TOpCode>;
        void Disconnect();
        void ProcessMessages();
        ShamanClientStatus GetStatus();
        int GetMessagesCountInQueue();
        int GetRtt();
        int GetPing();
        int GetMtu();
        Task<int> Ping(string address, ushort port, int timeoutMs = 1000);
        void Disconnect(byte[] data, int offset, int length);
    }
}