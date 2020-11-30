using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Tests
{
    public class MessagesConventionsTests
    {
        [Test]
        public void TestThatAllMessagesHasPublicDefaultConstructor()
        {
            var messageBaseType = typeof(PingRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) &&
                    !t.Namespace.Contains("RepositorySync") && // todo resolve "problem" with sync rep
                    !t.IsAbstract &&
                    (t.GetConstructor(Array.Empty<Type>()) == null))
                .ToArray();
            messageTypes.Should().BeEmpty();
        }
        [Test]
        public void TestThatAllMessagesHasUniqueOpeCode()
        {
            var messageBaseType = typeof(PingRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) 
                    && !t.IsAbstract
                    && !t.Namespace.Contains("RepositorySync"))
                .Select(t=>new Tuple<ushort, Type>(((MessageBase)Activator.CreateInstance(t)).OperationCode, t))
                .ToArray();

            var uniqueSet  = new Dictionary<ushort, Type>();
            var nonUniqueTuples = new List<Tuple<ushort, Type, Type>>();
            foreach (var tuple in messageTypes)
            {
                if (!uniqueSet.TryAdd(tuple.Item1, tuple.Item2))
                {
                    nonUniqueTuples.Add(new Tuple<ushort, Type, Type>(tuple.Item1, tuple.Item2,
                        uniqueSet[tuple.Item1]));
                }
            }

            nonUniqueTuples.Should().BeEmpty();
        }
    }
}