using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

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

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(PropertyCode);
            typeWriter.Write(ExactValue);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            PropertyCode = typeReader.ReadByte();
            ExactValue = typeReader.ReadInt();
        }
    }
}