using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Bro.WsShamanNetwork;

public class WebSocketServerTransport : ITransportLayer
{
    private readonly ITaskScheduler _taskScheduler;
    private readonly IShamanLogger _logger;
    private readonly HttpListener _httpListener;
    private IPendingTask _listener;
    private Action<IPEndPoint, DataPacket, Action> _onReceivePacket;
    private Func<IPEndPoint, bool> _onConnect;
    private Action<IPEndPoint, IDisconnectInfo> _onDisconnect;

    private static readonly byte[] PingPongLetter = Encoding.UTF8.GetBytes("p");
    #region Client-side

    public void Connect(IPEndPoint endPoint)
    {
    }

    public void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl,
        bool returnAfterSend = true)
    {
        throw new NotImplementedException();
    }

    public int GetPing()
    {
        throw new NotImplementedException();
    }

    public int GetRtt()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Close(byte[] data, int offset, int length)
    {
        throw new NotImplementedException();
    }

    public int Mtu { get; }
    public event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
    public event Action<IPEndPoint> OnConnected;
    public event Action<IPEndPoint, IDisconnectInfo> OnDisconnected;

    #endregion

    public WebSocketServerTransport(ITaskScheduler taskScheduler, IShamanLogger logger)
    {
        _taskScheduler = taskScheduler;
        _logger = logger;
        _httpListener = new HttpListener();
        _sendBufferPool = ArrayPool<byte>.Shared;
    }

    public void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket,
        Func<IPEndPoint, bool> onConnect, Action<IPEndPoint, IDisconnectInfo> onDisconnect)
    {
        _onDisconnect = onDisconnect;
        _onConnect = onConnect;
        _onReceivePacket = onReceivePacket;
    }

    private class Ctx
    {
        public HttpListenerWebSocketContext WebSocketContext { get; set; }
        public SemaphoreSlim Semaphore { get; set; }
    }

    private readonly ConcurrentDictionary<IPEndPoint, Ctx> _contexts = new();
    private readonly ArrayPool<byte> _sendBufferPool;

    public void Listen(int port)
    {
        var uri = $"http://*:{port}/";
        _httpListener.Prefixes.Add(uri);
        _httpListener.Start();
        _logger.Info($"WsServer listen to {uri}");
        _listener = _taskScheduler.Schedule(async () =>
        {
            while (true)
            {
                var ctx = await _httpListener.GetContextAsync();
                _logger.Info($"Incoming connection {ctx.Request.RemoteEndPoint}");
                if (!ctx.Request.IsWebSocketRequest)
                {
                    ctx.Response.StatusCode = 400;
                    ctx.Response.Close();
                    continue;
                }

                _ = _taskScheduler.Schedule(async () =>
                {
                    var ipEndPoint = ctx.Request.RemoteEndPoint;
                    var webSocketCtx = await ctx.AcceptWebSocketAsync(null);
                    try
                    {
                        var context = new Ctx
                        {
                            WebSocketContext = webSocketCtx,
                            Semaphore = new SemaphoreSlim(1, 1)
                        };
                        _contexts[ipEndPoint] = context;
                        _onConnect?.Invoke(ipEndPoint);
                        await HandleWsContext(context, ipEndPoint);
                    }
                    catch (WebSocketException e)
                    {
                        if (e.WebSocketErrorCode != WebSocketError.InvalidState &&
                            e.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                            _logger.Error($"WebSocket error: {e}");
                    }
                    finally
                    {
                        _contexts.TryRemove(ipEndPoint, out _);
                        try
                        {
                            webSocketCtx.WebSocket.Dispose();
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Failed to dispose socket: {e}");
                        }

                        _onDisconnect?.Invoke(ipEndPoint,
                            new SimpleDisconnectInfo(ShamanDisconnectReason.ConnectionLost));
                    }
                }, 0);
            }
        }, 0);
    }

    private async Task HandleWsContext(Ctx webSocketCtx, IPEndPoint ipEndPoint)
    {
        var webSocket = webSocketCtx.WebSocketContext.WebSocket;
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            var result =
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            // _logger.Error($"result: {result.EndOfMessage} {result.Count} {result.MessageType} {result.CloseStatus} {result.CloseStatusDescription}");
            if (result.CloseStatus.HasValue)
            {
                await webSocket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                    result.CloseStatusDescription,
                    CancellationToken.None);
                break;
            }

            if (result.MessageType == WebSocketMessageType.Close)
                break;
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var ping = buffer.AsSpan(0, result.Count).SequenceEqual(PingPongLetter);
                if (!ping)
                    break;

                await webSocketCtx.Semaphore.WaitAsync();
                try
                {
                    await webSocket.SendAsync(PingPongLetter, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                finally
                {
                    webSocketCtx.Semaphore.Release();
                }
                continue;
            }

            if (result.MessageType != WebSocketMessageType.Binary)
            {
                _logger.Error($"Unsupported message type: {result.MessageType}");
                continue;
            }

            var dataPacket = result.EndOfMessage
                ? new DataPacket(buffer, 0, result.Count, new DeliveryOptions(true, true))
                : await result.ReadBigMessage(_logger, buffer, webSocket);
            _onReceivePacket(ipEndPoint, dataPacket,
                () =>
                {
                    // consider data not used after processing the listener
                });
        }
    }

    public void Tick()
    {
        throw new NotImplementedException("Tick is not required for WebSocket transport");
    }

    public bool IsTickRequired => false;

    public void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl)
    {
        if (!_contexts.TryGetValue(endPoint, out var wctx))
            return;
        if (wctx.WebSocketContext.WebSocket.CloseStatus.HasValue)
            return;
        var webSocket = wctx.WebSocketContext.WebSocket;
        var cpBuffer = _sendBufferPool.Rent(length);
        Array.Copy(buffer, offset, cpBuffer, 0, length);
        var arraySegment = new ArraySegment<byte>(cpBuffer, 0, length);
        _ = SendFromRentedBuffer(wctx.Semaphore, endPoint, webSocket, arraySegment, cpBuffer);
    }

    private async Task SendFromRentedBuffer(SemaphoreSlim sendSemaphore, IPEndPoint endPoint, WebSocket webSocket,
        ArraySegment<byte> data, byte[] rentedBufferToRelease)
    {
        try
        {
            try
            {
                await sendSemaphore.WaitAsync();
                await webSocket.SendAsync(data, WebSocketMessageType.Binary, true,
                    CancellationToken.None);
            }
            catch (Exception e)
            {
                if (_contexts.ContainsKey(endPoint) && !webSocket.CloseStatus.HasValue)
                    _logger.Error($"Failed to send data to peer {endPoint}: {e}");
            }
            finally
            {
                sendSemaphore.Release();
            }
        }
        finally
        {
            _sendBufferPool.Return(rentedBufferToRelease);
        }
    }

    public bool DisconnectPeer(IPEndPoint ipEndPoint)
    {
        if (!_contexts.TryGetValue(ipEndPoint, out var wctx))
            return false;
        return DisconnectPeerImpl(wctx.WebSocketContext);
    }

    private static bool DisconnectPeerImpl(HttpListenerWebSocketContext wctx)
    {
        if (wctx.WebSocket.CloseStatus.HasValue)
            return false;
        wctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        return true;
    }

    public bool DisconnectPeer(IPEndPoint ipEndPoint, byte[] data, int offset, int length)
    {
        if (!_contexts.TryGetValue(ipEndPoint, out var wctx))
            return false;
        if (wctx.WebSocketContext.WebSocket.CloseStatus.HasValue)
            return false;
        wctx.WebSocketContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
            Convert.ToBase64String(data, offset, length), CancellationToken.None);
        return true;
    }
}
