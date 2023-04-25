using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
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
    }

    public void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket,
        Func<IPEndPoint, bool> onConnect, Action<IPEndPoint, IDisconnectInfo> onDisconnect)
    {
        _onDisconnect = onDisconnect;
        _onConnect = onConnect;
        _onReceivePacket = onReceivePacket;
    }

    private readonly ConcurrentDictionary<IPEndPoint, HttpListenerWebSocketContext> _contexts = new();

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
                }

                var ipEndPoint = ctx.Request.RemoteEndPoint;
                var webSocketCtx = await ctx.AcceptWebSocketAsync(null);
                _contexts[ipEndPoint] = webSocketCtx;
                _onConnect?.Invoke(ipEndPoint);
                _ = _taskScheduler.Schedule(async () => { await HandleWsContext(webSocketCtx, ipEndPoint); }, 0);
            }
        }, 0);
    }

    private async Task HandleWsContext(HttpListenerWebSocketContext webSocketCtx, IPEndPoint ipEndPoint)
    {
        var webSocket = webSocketCtx.WebSocket;
        var buffer = new byte[1024];
        WebSocketReceiveResult result =
            await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            if (!result.EndOfMessage)
            {
                _logger.Error("Message is too long");
                await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message is too long",
                    CancellationToken.None);
                _onDisconnect?.Invoke(ipEndPoint, new SimpleDisconnectInfo(ShamanDisconnectReason.ConnectionLost));
                return;
            }

            var dataPacket = new DataPacket(buffer, 0, result.Count, new DeliveryOptions(true, true));

            _onReceivePacket(ipEndPoint, dataPacket, () =>
            {
                // consider data not used after processing the listener
            });
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        Console.WriteLine("WebSocket connection closed");
    }

    public void Tick()
    {
    }

    public void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl)
    {
        if (!_contexts.TryGetValue(endPoint, out var wctx))
            return;
        var webSocket = wctx.WebSocket;
        webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, length), WebSocketMessageType.Binary, true,
                CancellationToken.None)
            .ContinueWith(HandleWsSend, endPoint);
    }

    private void HandleWsSend(Task sendingTask, object ip)
    {
        if (!sendingTask.IsFaulted)
            return;
        _logger.Error($"Failed to send data to peer {ip}: {sendingTask.Exception}");
    }

    public bool DisconnectPeer(IPEndPoint ipEndPoint)
    {
        if (!_contexts.TryGetValue(ipEndPoint, out var wctx))
            return false;
        wctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", CancellationToken.None);
        return true;
    }

    public bool DisconnectPeer(IPEndPoint ipEndPoint, byte[] data, int offset, int length)
    {
        if (!_contexts.TryGetValue(ipEndPoint, out var wctx))
            return false;
        wctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
            "DisconnectWithPl:" + Convert.ToBase64String(data, offset, length), CancellationToken.None);
        return true;
    }
}