using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Bro.WsShamanNetwork;

public class WebSocketClientTransport : ITransportLayer
{
    private readonly ITaskScheduler _taskScheduler;
    private readonly IShamanLogger _logger;
    private readonly ClientWebSocket _clientWebSocket;

    public WebSocketClientTransport(ITaskScheduler taskScheduler, IShamanLogger logger)
    {
        _taskScheduler = taskScheduler;
        _logger = logger;
        _clientWebSocket = new ClientWebSocket();
    }

    #region Client-side

    public void Connect(IPEndPoint endPoint)
    {
        _taskScheduler.Schedule(async () =>
        {
            var uri = new Uri($"ws://{endPoint}");
            _logger.Info($"WsClient connecting to {uri}");
            await _clientWebSocket.ConnectAsync(uri,
                CancellationToken.None // todo possible handle timeout
            );
            if (_clientWebSocket.State == WebSocketState.Open)
                OnConnected?.Invoke(endPoint);

            try
            {
                while (_clientWebSocket.State == WebSocketState.Open)
                {
                    var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
                    var result =
                        await _clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                            CancellationToken.None);
                        break;
                    }

                    if (result.MessageType != WebSocketMessageType.Binary)
                    {
                        _logger.Error($"Unsupported message type: {result.MessageType}");
                        continue;
                    }


                    var dataPacket = result.EndOfMessage
                        ? new DataPacket(buffer, 0, result.Count, new DeliveryOptions(true, true))
                        : await result.ReadBigMessage(_logger, buffer, _clientWebSocket);
                    OnPacketReceived?.Invoke(endPoint, dataPacket,
                        () => ArrayPool<byte>.Shared.Return(buffer)); // todo refactor buffer disposing to avoid closure
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Peer connection processing failed: {e}");
            }
            finally
            {
                OnDisconnected?.Invoke(endPoint,
                    new SimpleDisconnectInfo(ShamanDisconnectReason.ConnectionLost));
            }
        }, 0);
    }

    public void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl,
        bool returnAfterSend = true)
    {
        if (_clientWebSocket.State != WebSocketState.Open)
            return;
        _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer, offset, length), WebSocketMessageType.Binary,
            true, CancellationToken.None).ContinueWith(HandleResult);
    }

    private void HandleResult(Task sendingTask)
    {
        if (!sendingTask.IsFaulted)
            return;
        if (_clientWebSocket.State != WebSocketState.Open)
            return;
        _logger.Error($"Failed to send data to peer: {sendingTask.Exception}");
    }


    public int GetPing()
    {
        //todo
        return 100;
    }

    public int GetRtt()
    {
        //todo
        return 100;
    }

    public void Close()
    {
        // todo implement reason according to server impl
        if (_clientWebSocket.State != WebSocketState.Open)
            _clientWebSocket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None)
                .ContinueWith(HandleResult);
        OnDisconnected?.Invoke(null, new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave));
    }

    public void Close(byte[] data, int offset, int length)
    {
        // todo implement payload according to server impl
        if (_clientWebSocket.State != WebSocketState.Open)
            _clientWebSocket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None)
                .ContinueWith(HandleResult);
        OnDisconnected?.Invoke(null, new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave));
    }


    // why do we need IP here?
    public int Mtu { get; }
    public event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
    public event Action<IPEndPoint> OnConnected;
    public event Action<IPEndPoint, IDisconnectInfo> OnDisconnected;

    #endregion

    #region Server-side

    public void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket,
        Func<IPEndPoint, bool> onConnect, Action<IPEndPoint, IDisconnectInfo> onDisconnect)
    {
        throw new NotImplementedException();
    }

    public void Listen(int port)
    {
        throw new NotImplementedException();
    }

    public void Tick()
    {
        throw new NotImplementedException("Tick is not required for WebSocket transport");
    }

    public bool IsTickRequired => false;

    public void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl)
    {
        throw new NotImplementedException();
    }

    public bool DisconnectPeer(IPEndPoint ipEndPoint)
    {
        throw new NotImplementedException();
    }

    public bool DisconnectPeer(IPEndPoint ipEndPoint, byte[] data, int offset, int length)
    {
        throw new NotImplementedException();
    }

    #endregion
}