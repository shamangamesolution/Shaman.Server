using System.Collections.Generic;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Launchers.Tests;

public class TestEvent : MessageBase
{
    public TestEvent() : base(101)
    {
    }

    public int IntValue { get; set; }

    protected override void SerializeBody(ITypeWriter typeWriter)
    {
        typeWriter.Write(IntValue);
    }

    protected override void DeserializeBody(ITypeReader typeReader)
    {
        IntValue = typeReader.ReadInt();
    }
}
public class HeavyTestEvent : MessageBase
{
    public HeavyTestEvent() : base(102)
    {
    }

    public string StringValue { get; set; }
    public Dictionary<string,string> StringDictionary { get; set; }

    protected override void SerializeBody(ITypeWriter typeWriter)
    {
        typeWriter.Write(StringValue);
        if (StringDictionary == null)
            typeWriter.Write(0);
        else
        {
            typeWriter.Write(StringDictionary.Count);
            foreach (var item in StringDictionary)
            {
                typeWriter.Write(item.Key);
                typeWriter.Write(item.Value);
            }
        }

    }

    protected override void DeserializeBody(ITypeReader typeReader)
    {
        StringValue = typeReader.ReadString();
        StringDictionary = new Dictionary<string, string>();
        var count = typeReader.ReadInt();
        for (var i = 0; i < count; i++)
        {
            var key = typeReader.ReadString();
            var value = typeReader.ReadString();
            StringDictionary.Add(key, value);
        }
    }
}