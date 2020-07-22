using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Contract;

namespace Shaman.Contract.Bundle
{
    public interface IGameModeController
    {
        Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties);
        void CleanupPlayer(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload);

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
        void Cleanup();
        void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId);
    }
}