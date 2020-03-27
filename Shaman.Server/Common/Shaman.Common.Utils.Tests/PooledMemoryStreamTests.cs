using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Pooling;

namespace Shaman.Common.Utils.Tests
{
    [TestFixture]
    public class PooledMemoryStreamTests
    {
        class TestDto : ISerializable
        {
            public string Data1 { get; set; }
            public int Data2 { get; set; }
            public byte[] Data3 { get; set; }

            public void Serialize(ITypeWriter typeWriter)
            {
                typeWriter.Write(Data1);
                typeWriter.Write(Data2);
                typeWriter.Write(Data3);
            }

            public void Deserialize(ITypeReader typeReader)
            {
                Data1 = typeReader.ReadString();
                Data2 = typeReader.ReadInt();
                Data3 = typeReader.ReadBytes();
            }
        }

        private const int Cycles = 10000000;
        private const int ObjectsCount = 100;

        private static readonly List<TestDto> Objects = new List<TestDto>();

        static PooledMemoryStreamTests()
        {
            var fixture = new Fixture();
            for (int i = 0; i < ObjectsCount; i++)
            {
                Objects.Add(fixture.Create<TestDto>());
            }

            GC.Collect();
        }

        [Test]
        [Ignore("Only for manual run")]
        public void PerformanceTestWithPooling()
        {
            var serializer = new BinarySerializer();
            Console.Out.WriteLine("GC.GetTotalMemory(false) = {0}", GC.GetTotalMemory(false));
            long size = 0;
            for (int i = 0; i < Cycles; i++)
            {
                using (var memoryStream = new PooledMemoryStream(128))
                {
                    serializer.Serialize(Objects[i % ObjectsCount], memoryStream);
                    size += memoryStream.Position;
                }
            }

            // here test was break-pointed and analyzed with profiler
            Console.Out.WriteLine("GC.GetTotalMemory(false) = {0}", GC.GetTotalMemory(false));
            Console.Out.WriteLine("size = {0}", size / Cycles);
        }

        [Test]
        [Ignore("Only for manual run")]
        public void PerformanceTestWithoutPooling()
        {
            var serializer = new BinarySerializer();
            Console.Out.WriteLine("GC.GetTotalMemory(false) = {0}", GC.GetTotalMemory(false));
            long size = 0;
            for (int i = 0; i < Cycles; i++)
            {
                var data = serializer.Serialize(Objects[i % ObjectsCount]);
                size += data.Length;
            }

            // here test was break-pointed and analyzed with profiler
            Console.Out.WriteLine("GC.GetTotalMemory(false) = {0}", GC.GetTotalMemory(false));
            Console.Out.WriteLine("size = {0}", size / Cycles);
        }

        [Test]
        public void SerializeCycleTest()
        {
            var serializer = new BinarySerializer();
            {
                using (var memoryStream = new PooledMemoryStream(128))
                {
                    serializer.Serialize(Objects[0], memoryStream);
                    var data = memoryStream.GetBuffer();
                    var dto = serializer.DeserializeAs<TestDto>(data, 0, (int) memoryStream.Length);
                    dto.Should().BeEquivalentTo(Objects[0]);
                }
            }
        }
    }
}