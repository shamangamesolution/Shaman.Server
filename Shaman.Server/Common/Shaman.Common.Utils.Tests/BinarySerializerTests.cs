using System;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Serialization;

namespace Shaman.Common.Utils.Tests
{
    public class TestDto : ISerializable
    {
        public string Data1 { get; set; }
        public int Data2 { get; set; }

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(Data1);
            typeWriter.Write(Data2);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            Data1 = typeReader.ReadString();
            Data2 = typeReader.ReadInt();
        }
    }

    public class BinarySerializerTests
    {
        [Test]
        public void Test()
        {
            var binarySerializer = new BinarySerializer();
            var testDto = new TestDto {Data1 = "test1", Data2 = 12};
            var data = binarySerializer.Serialize(testDto);
            binarySerializer.DeserializeAs<TestDto>(data).Should().BeEquivalentTo(testDto);

            var moreData = new byte[data.Length + 5];
            
            Array.Copy(data,0,moreData,2, data.Length);
            
            binarySerializer.DeserializeAs<TestDto>(moreData, 2, data.Length).Should().BeEquivalentTo(testDto);
        }
    }
}