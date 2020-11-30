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
                var reasonPayload = ServerDisconnectReasonPayloadHelper.GetReasonPayload(value);
                reasonPayload[0].Should().Be((byte) value);
            }
        }
    }
}