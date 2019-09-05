using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity
{
    public struct ParameterNames
    {
        public const string IsOnService = "IsOnService";
    }

    [Serializable]
    public class GlobalParameter : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StringValue { get; set; } = "";
        public int? IntValue { get; set; }
        public float? FloatValue { get; set; }
        public bool? BoolValue { get; set; }
        public DateTime? DateTimeValue { get; set; }

        public int GetIntValue()
        {
            if (IntValue != null)
                return IntValue.Value;
            else
                throw new Exception("Parameter has null value");
        }
        public string GetStringValue()
        {
            if (StringValue != null)
                return StringValue;
            else
                throw new Exception("Parameter has null value");
        }
        public float GetFloatValue()
        {
            if (FloatValue != null)
                return FloatValue.Value;
            else
                throw new Exception("Parameter has null value");
        }
        public bool GetBoolValue()
        {
            if (BoolValue != null)
                return BoolValue.Value;
            else
                throw new Exception("Parameter has null value");
        }
        
        public DateTime? GetNullableDateTimeValue()
        {
            return DateTimeValue;
        }

        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(this.Id);
            serializer.Write(this.Name);
            serializer.Write(this.StringValue);
            if (IntValue != null)
            {
                serializer.Write((byte) 1);
                serializer.Write(this.IntValue.Value);
            }
            else
            {
                serializer.Write((byte) 0);
            }
            if (FloatValue != null)
            {
                serializer.Write((byte) 1);
                serializer.Write(this.FloatValue.Value);
            }
            else
            {
                serializer.Write((byte) 0);
            }
            if (BoolValue != null)
            {
                serializer.Write((byte) 1);
                serializer.Write(this.BoolValue.Value);
            }
            else
            {
                serializer.Write((byte) 0);
            }
            if (DateTimeValue != null)
            {
                serializer.Write((byte) 1);
                serializer.Write(this.DateTimeValue.Value.ToBinary());
            }
            else
            {
                serializer.Write((byte) 0);
            }
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            Id = serializer.ReadInt();
            Name = serializer.ReadString();
            StringValue = serializer.ReadString();
            var intValueSet = serializer.ReadByte();
            if (intValueSet == 1)
                IntValue = serializer.ReadInt();
            var floatValueSet = serializer.ReadByte();
            if (floatValueSet == 1)
                FloatValue = serializer.ReadFloat();
            var boolValueSet = serializer.ReadByte();
            if (boolValueSet == 1)
                BoolValue = serializer.ReadBool();
            var dtValueSet = serializer.ReadByte();
            if (dtValueSet == 1)
                DateTimeValue = new DateTime(serializer.ReadLong());
        }
        
    }
}
