using System;
using System.Collections.Generic;
using System.Linq;

namespace Shaman.Common.Server.Peers
{
    public static class ServerDisconnectReasonPayloadHelper
    {
        private static readonly IDictionary<ServerDisconnectReason, byte[]> ReasonPayloadCache =
            CreateReasonPayloadCache();

        private static IDictionary<ServerDisconnectReason, byte[]> CreateReasonPayloadCache()
        {
            return Enum.GetValues(typeof(ServerDisconnectReason))
                .OfType<ServerDisconnectReason>()
                .ToDictionary(k => k, v => new[] {(byte) v});
        }

        public static byte[] ToPayload(this ServerDisconnectReason reason)
        {
            return ReasonPayloadCache[reason];
        }

        public static ServerDisconnectReason ServerDisconnectReason(this byte[] payload, int offset)
        {
            return (ServerDisconnectReason) payload[offset];
        }
    }
}