using System;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Messages.Handling;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Tests
{
    public class TestMessage1 : MessageBase
    {
        public TestMessage1() : base(1)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
        }
    }

    public class TestMessage2 : MessageBase
    {
        public TestMessage2() : base(2)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
        }
    }

    public class TestFailMessage : MessageBase
    {
        public TestFailMessage() : base(3)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
        }
    }

    class TestMessagesDispatcher : MessagesDispatcherBase<TestMessagesHandler>,
        IMessageHandler<TestMessage1, TestMessagesHandler>,
        IMessageHandler<TestMessage2, TestMessagesHandler>,
        IMessageHandler<TestFailMessage, TestMessagesHandler>
    {
        public bool Handle(TestMessage1 message, Guid sessionId, TestMessagesHandler ctx)
        {
            return ctx.Handle(message, sessionId);
        }

        public bool Handle(TestMessage2 message, Guid sessionId, TestMessagesHandler ctx)
        {
            return ctx.Handle(message, sessionId);
        }

        public bool Handle(TestFailMessage message, Guid sessionId, TestMessagesHandler ctx)
        {
            return ctx.Handle(message, sessionId);
        }

        protected override void Initialize()
        {
            RegisterHandler<TestMessage1>(this);
            RegisterHandler<TestMessage2>(this);
            RegisterHandler<TestFailMessage>(this);
        }
    }

    class TestMessagesHandler
    {
        public bool TestMessage1Processed { get; set; }
        public bool TestMessage2Processed { get; set; }

        public bool Handle(TestMessage2 message, Guid sessionId)
        {
            TestMessage2Processed = true;
            return true;
        }

        public bool Handle(TestMessage1 message, Guid sessionId)
        {
            TestMessage1Processed = true;
            return true;
        }

        public bool Handle(TestFailMessage message, Guid sessionId)
        {
            return false;
        }
    }

    public class MessageRouterTest
    {
        [Test]
        public void Test()
        {
            var processor = MessagesRouterFactory.Create<TestMessagesHandler, TestMessagesDispatcher>();
            var handler = new TestMessagesHandler();

            handler.TestMessage1Processed.Should().BeFalse();
            handler.TestMessage2Processed.Should().BeFalse();

            var serializer = new BinarySerializer();

            var testMessage1 = new TestMessage1();
            var testMessage2 = new TestMessage2();
            var testFailMessage = new TestFailMessage();
            var testMessage1Data = serializer.Serialize(testMessage1);
            var testMessage2Data = serializer.Serialize(testMessage2);
            var testFailMessageData = serializer.Serialize(testFailMessage);

            processor.Route(serializer, testMessage1.OperationCode, testMessage1Data, 0, testMessage1Data.Length, Guid.Empty,
                    handler).Handled
                .Should().BeTrue();
            handler.TestMessage1Processed.Should().BeTrue();
            handler.TestMessage2Processed.Should().BeFalse();

            processor.Route(serializer, testMessage2.OperationCode, testMessage2Data, 0, testMessage2Data.Length, Guid.Empty,
                    handler).Handled
                .Should().BeTrue();
            handler.TestMessage1Processed.Should().BeTrue();
            handler.TestMessage2Processed.Should().BeTrue();

            handler.TestMessage1Processed.Should().BeTrue();
            handler.TestMessage2Processed.Should().BeTrue();

            processor.Route(serializer, testFailMessage.OperationCode, testFailMessageData, 0, testFailMessageData.Length, Guid.Empty,
                    handler).Handled.Should().BeFalse();
        }
    }
}