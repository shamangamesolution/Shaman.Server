using System;
using System.IO;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;
using Shaman.Serialization.Messages.Udp;

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
            var messageBaseType = typeof(PingRequest);
            var messages = messageBaseType.Assembly.GetTypes().Where(t =>
                t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                t.GetConstructor(Array.Empty<Type>()) != null).Select(CreateFromType).OfType<ISerializable>().ToArray();


            //todo fix tests
            foreach (var message in messages)
            {
                Console.Out.WriteLine("Testing '{0}'...", message.GetType());
                if (message is ResponseBase)
                {
                    ((ResponseBase) message).ResultCode = ResultCode.OK;
                }
                else if (message is HttpResponseBase)
                {
                    ((HttpResponseBase) message).ResultCode = ResultCode.OK;
                }
                
                var serialized = Serialize(message);
                var deserialized = Deserialize(serialized, (MessageBase) Activator.CreateInstance(message.GetType()));
                deserialized.Should().BeEquivalentTo((object)/*cast to force deep check*/message, $"Deserialized type {serialized.GetType()}");
            }

            CreateFromType(typeof(PingRequest));
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