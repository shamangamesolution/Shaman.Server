using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Messages
{
    public enum PlayerAction : byte
    {
        Joined,
        Leave
    }

    public class PlayerEvent : EventBase
    {
        public Guid PlayerId { get; set; }
        public PlayerAction Action { get; set; }


        public PlayerEvent() : base(MessageCodes.PlayerJoinedEvent)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(PlayerId);
            typeWriter.Write((byte) Action);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            PlayerId = typeReader.ReadGuid();
            Action = (PlayerAction) typeReader.ReadByte();
        }
    }
}