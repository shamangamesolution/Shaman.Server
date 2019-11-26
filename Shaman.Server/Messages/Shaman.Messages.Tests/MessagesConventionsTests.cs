using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Messages;
using Shaman.Messages.RW.DTO.Request;
using Shaman.Messages.RW.Entity.Statistics;

namespace Shaman.Messages.Tests
{
    public class MessagesConventionsTests
    {
        [Test]
        public void TestThatAllMessagesHasPublicDefaultConstructor()
        {
            var messageBaseType = typeof(BuyRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) == null)
                .ToArray();
            messageTypes.Should().BeEmpty();
        }
    }
}