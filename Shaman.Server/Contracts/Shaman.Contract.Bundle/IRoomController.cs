using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Contract.Common;

namespace Shaman.Contract.Bundle
{
    public interface IRoomController: IDisposable
    {
        /// <summary>
        /// Note: controller should handle case when player disconnected during ProcessNewPlayer
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties);
        void ProcessPlayerDisconnected(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload);
        /// <returns>true if room should be destroyed</returns>
        bool IsGameFinished();

        /// <summary>
        /// Time when room should be destroyed forcibly
        ///
        /// return TimeSpan.MaxValue to avoid forcibly destroy
        /// </summary>
        TimeSpan ForceDestroyRoomAfter { get; }

        /// <summary>
        /// Cleanup here all allocated resources
        /// </summary>
        void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId);
        
        int MaxMatchmakingWeight { get; }
    }
}