using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shaman.Game.Contract
{
    public interface IGameModeController
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
        void Cleanup();
        void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId);
    }
}