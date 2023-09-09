using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Bro.WsShamanNetwork;

public class WebSocketClientTransport : ITransportLayer
{
    private readonly ITaskScheduler _taskScheduler;
    private readonly IShamanLogger _logger;
    private readonly ClientWebSocket _clientWebSocket;
    private static readonly byte[] PingPongLetter = Encoding.UTF8.GetBytes("p");

    private int _rtt = 0;
    private bool _waitForPong = false;
    private DateTime _pingSent = DateTime.UtcNow;
    private readonly TimeSpan KeepAliveInterval;

    public WebSocketClientTransport(ITaskScheduler taskScheduler, IShamanLogger logger, TimeSpan keepAliveInterval)
    {
        _taskScheduler = taskScheduler;
        _logger = logger;
        _clientWebSocket = new ClientWebSocket();
        KeepAliveInterval = keepAliveInterval;
    }

    #region Client-side

    public void Connect(IPEndPoint endPoint)
    {
        _taskScheduler.Schedule(async () =>
        {
            try
            {
                var uri = new Uri($"ws://{endPoint}");
                _logger.Info($"WsClient connecting to {uri}");
                await _clientWebSocket.ConnectAsync(uri,
                    CancellationToken.None // todo possible handle timeout
                );
                if (_clientWebSocket.State == WebSocketState.Open)
                    OnConnected?.Invoke(endPoint);
                await SendHb();
                var cancellationToken = new CancellationTokenSource();
                while (_clientWebSocket.State == WebSocketState.Open)
                {
                    var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
                    cancellationToken.CancelAfter(KeepAliveInterval);
                    var result =
                        await _clientWebSocket.ReceiveAsync(buffer, cancellationToken.Token);
                    // _logger.Error($"result: {result.EndOfMessage} {result.Count} {result.MessageType} {result.CloseStatus} {result.CloseStatusDescription}");

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var ping = buffer.AsSpan(0, result.Count).SequenceEqual(PingPongLetter);
                        if (!ping)
                        {
                            _logger.Error("Bad pong received");
                            break;
                        }

                        _rtt = (int) (DateTime.UtcNow - _pingSent).TotalMilliseconds;
                        _waitForPong = false;
                        _logger.Info($"PONG received3 ({_rtt})");
                        _taskScheduler.Schedule(async () => { await SendHb(); }, Math.Max(0, (long) (1000 - _rtt)));
                        continue;
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
                OnDisconnected?.Invoke(endPoint,
                    new SimpleDisconnectInfo(ShamanDisconnectReason.ConnectionLost));
            }
        }, 0);
    }

    private async Task SendHb()
    {
        _pingSent = DateTime.UtcNow;
        _waitForPong = true;
        try
        {
            await _clientWebSocket.SendAsync(PingPongLetter, WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
        catch (Exception e)
        {
            // no worries
        }
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
        if (_waitForPong)
        {
            var rtt = (int) (DateTime.UtcNow - _pingSent).TotalMilliseconds;
            return Math.Max(rtt, _rtt);
        }

        return _rtt;
    }

    public int GetRtt()
    {
        return GetPing();
    }

    public void Close()
    {
        if (_clientWebSocket.State == WebSocketState.Open)
            _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                .ContinueWith(HandleResult)
                .ContinueWith(_ =>
                    OnDisconnected?.Invoke(null, new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave)));
    }

    public void Close(byte[] data, int offset, int length)
    {
        if (_clientWebSocket.State == WebSocketState.Open)
            _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    Convert.ToBase64String(data, offset, length), CancellationToken.None)
                .ContinueWith(HandleResult)
                .ContinueWith(_ =>
                {
                    OnDisconnected?.Invoke(null, new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave));
                });
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