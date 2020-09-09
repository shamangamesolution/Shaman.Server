using System;
using System.Collections.Generic;
using System.Linq;

namespace Shaman.Common.Server.Peers
{
    public class ServerDisconnectReasonPayloadHelper
    {
        private static readonly IDictionary<ServerDisconnectReason, byte[]> ReasonPayloadCache =
            CreateReasonPayloadCache();

        private static IDictionary<ServerDisconnectReason, byte[]> CreateReasonPayloadCache()
        {
            return Enum.GetValues(typeof(ServerDisconnectReason))
                .OfType<ServerDisconnectReason>()
                .ToDictionary(k => k, v => new[] {(byte) v});
        }

        public static byte[] GetReasonPayload(ServerDisconnectReason reason)
        {
            return ReasonPayloadCache[reason];
        }
    }
}