using System;
using System.IO;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.RW.DTO.Request;

namespace Shaman.Messages.Tests
{
    class SerializationTests
    {
        private static readonly Fixture Fixture = new Fixture();

        static SerializationTests()
        {
            var random = new Random();

            // mocking IRobotAbilityManager in RobotData, for example
            Fixture.Customize(new AutoMoqCustomization());

            // fill dictionaries with supported objects 
            Fixture.Customize<object>(o => o.FromFactory(() => (object) random.Next()));
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            var messageBaseType = typeof(BuyRequest);
            var messages = messageBaseType.Assembly.GetTypes().Where(t =>
                t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                t.GetConstructor(Array.Empty<Type>()) != null).Select(CreateFromType).OfType<MessageBase>().ToArray();


            foreach (var message in messages)
            {
                Console.Out.WriteLine("Testing '{0}'...", message.GetType());
                var serialized = Serialize(message);
                var deserialized = Deserialize(serialized, (MessageBase) Activator.CreateInstance(message.GetType()));
                deserialized.Should().BeEquivalentTo(message, $"Deserialized type {serialized.GetType()}");
            }

            CreateFromType(typeof(BuyRequest));
        }

        private object CreateFromType(Type request)
        {
            var specimenContext = new SpecimenContext(Fixture);
            return specimenContext.Resolve(request);
        }

        public byte[] Serialize(ISerializable serializable)
        {
            var memoryStream = new MemoryStream();
            using (var bw = new BinaryWriter(memoryStream))
            {
                serializable.Serialize(new BinaryTypeWriter(bw));
            }

            return memoryStream.ToArray();
        }

        public T Deserialize<T>(byte[] param, T result)
            where T : ISerializable
        {
            using (var reader = new BinaryReader(new MemoryStream(param)))
            {
                var deserializer = new BinaryTypeReader(reader);
                result.Deserialize(deserializer);
            }

            return result;
        }
    }
}