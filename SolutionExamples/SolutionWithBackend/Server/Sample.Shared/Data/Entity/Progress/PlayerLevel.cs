using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Progress
{
    public class PlayerLevel : EntityBase
    {
        public int Level { get; set; }
        public int Experience { get; set; }
        
        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(Level);
            serializer.Write(Experience);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            Level = serializer.ReadInt();
            Experience = serializer.ReadInt();
        }
    }
}