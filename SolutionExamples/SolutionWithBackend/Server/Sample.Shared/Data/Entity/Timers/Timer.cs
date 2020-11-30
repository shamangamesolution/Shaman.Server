using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Timers
{
    public enum TimerType : byte
    {
        PerkDelivery = 1
    }
    
    public class Timer : EntityBase
    {
        public TimerType Type { get; set; }
        public DateTime StartedOn { get; set; }
        public int SecondsToComplete { get; set; }
        public int PlayerId { get; set; }
        public int RelatedObjectId { get; set; }
        
        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write((byte)this.Type);
            serializer.Write(this.StartedOn.ToBinary());
            serializer.Write(this.SecondsToComplete);
            serializer.Write(this.PlayerId);
            serializer.Write(this.RelatedObjectId);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            this.Type = (TimerType) serializer.ReadByte();
            this.StartedOn = DateTime.FromBinary(serializer.ReadLong());
            this.SecondsToComplete = serializer.ReadInt();
            this.PlayerId = serializer.ReadInt();
            this.RelatedObjectId = serializer.ReadInt();
        }
    }
}