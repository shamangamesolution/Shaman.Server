using System;
using LiteNetLib;
using Shaman.Common.Udp.Sockets;

namespace Shaman.LiteNetLibAdapter
{
    public class LightNetDisconnectInfo : IDisconnectInfo
    {
        private readonly NetPacketReader _payload;
        public DisconnectReason DisconnectReason { get; private set; }

        public LightNetDisconnectInfo(DisconnectInfo info)
        {
            _payload = info.AdditionalData;
            DisconnectReason = info.Reason;
            ShamanServerReason = GetServerDisconnectReason(info);
        }

        private static ServerDisconnectReason? GetServerDisconnectReason(DisconnectInfo info)
        {
            return info.Reason == DisconnectReason.RemoteConnectionClose 
                   && info.AdditionalData != null 
                   && info.AdditionalData.UserDataSize == 1 
                ? ParseServerDisconnectReason(info)
                : null;
        }

        private static ServerDisconnectReason? ParseServerDisconnectReason(DisconnectInfo info)
        {
            try
            {
                return (ServerDisconnectReason?) info.AdditionalData.RawData[info.AdditionalData.UserDataOffset];
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _payload?.Recycle();
        }

        public ServerDisconnectReason? ShamanServerReason { get; }
        
        //todo will be removed
        public ClientDisconnectReason Reason { get; }
        public byte[] Payload { get; }
    }
}