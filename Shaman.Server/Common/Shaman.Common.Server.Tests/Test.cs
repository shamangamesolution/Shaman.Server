using System;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Server.Peers;

namespace Shaman.Common.Server.Tests
{
    public class ServerDisconnectReasonPayloadHelperTests
    {
        [Test]
        public void ReasonPayloadTest()
        {
            foreach (ServerDisconnectReason value in Enum.GetValues(typeof(ServerDisconnectReason)))
            {
                var reasonPayload = value.ToPayload();
                reasonPayload[0].Should().Be((byte) value);
            }
        }
    }
}