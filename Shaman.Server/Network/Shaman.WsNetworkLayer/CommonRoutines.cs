using System.Net.WebSockets;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Bro.WsShamanNetwork;

static class CommonRoutines
{
    public static async Task<DataPacket> ReadBigMessage(this WebSocketReceiveResult result, IShamanLogger logger,
        byte[] buffer, WebSocket webSocket)
    {
        var msgSize = result.Count;
        var oldBuffer = buffer;
        do
        {
            logger.Warning($"Expanding server receive buffer to {oldBuffer.Length * 2}");
            var expandedBuffer = new byte[oldBuffer.Length * 2];
            Buffer.BlockCopy(oldBuffer, 0, expandedBuffer, 0, oldBuffer.Length);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(expandedBuffer, oldBuffer.Length,
                expandedBuffer.Length - oldBuffer.Length), CancellationToken.None);
            msgSize += result.Count;
            oldBuffer = expandedBuffer;
        } while (!result.EndOfMessage);

        return new DataPacket(oldBuffer, 0, msgSize, new DeliveryOptions(true, true));
    }
}