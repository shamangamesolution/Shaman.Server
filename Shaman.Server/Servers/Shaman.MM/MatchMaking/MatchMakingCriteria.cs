using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.MM.MatchMaking
{

    public class MatchMakingMeasure : EntityBase
    {        
        public byte PropertyCode { get; set; }
        public int ExactValue { get; set; }

        
        public MatchMakingMeasure(byte propertyCode, int exactValue)
        {
            PropertyCode = propertyCode;
            ExactValue = exactValue;
        }

        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(PropertyCode);
            serializer.Write(ExactValue);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            PropertyCode = serializer.ReadByte();
            ExactValue = serializer.ReadInt();
        }
    }
}