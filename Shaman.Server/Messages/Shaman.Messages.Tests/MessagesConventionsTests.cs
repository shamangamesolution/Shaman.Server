using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Messages;
using Shaman.Messages.General.DTO.Requests;

namespace Shaman.Messages.Tests
{
    public class MessagesConventionsTests
    {
        [Test]
        public void TestThatAllMessagesHasPublicDefaultConstructor()
        {
            var messageBaseType = typeof(PingRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) == null)
                .ToArray();
            messageTypes.Should().BeEmpty();
        }
    }
}