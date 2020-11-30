using System.Linq;
using NUnit.Framework;
using Shaman.Contract.Routing;

namespace Shaman.Tests
{
    [TestFixture]
    public class RoutingTests
    {
        [Test]
        public void BackCompatibilityTest()
        {
            var serverInfo1 = new ServerInfo {ClientVersion = "0.0.1"};
            var serverInfo2 = new ServerInfo {ClientVersion = "0.0.1"};
            var clientVersionList1 = serverInfo1.ClientVersionList.ToList();
            var clientVersionList2 = serverInfo2.ClientVersionList.ToList();
            Assert.AreEqual(1, clientVersionList1.Count);
            Assert.IsTrue(clientVersionList1.Contains("0.0.1"));
            Assert.AreEqual(1, clientVersionList2.Count);
            Assert.IsTrue(clientVersionList2.Contains("0.0.1"));
            
            Assert.IsTrue(serverInfo1.AreVersionsIntersect(serverInfo2));
            var intersection = serverInfo1.GetVersionIntersection(serverInfo2).ToList();
            Assert.AreEqual(1, intersection.Count());
            Assert.IsTrue(intersection.Contains("0.0.1"));
        }

        [Test]
        public void IntersectionTest()
        {
            var serverInfo1 = new ServerInfo {ClientVersion = "1,12,2, 112, 1 , 0.0.112,4"};
            var serverInfo2 = new ServerInfo {ClientVersion = "5,jkl, ,0.0.1122, 0.1, 12 0.0.112,0.0.112, 12 "};
            var clientVersionList1 = serverInfo1.ClientVersionList.ToList();
            var clientVersionList2 = serverInfo2.ClientVersionList.ToList();
            Assert.AreEqual(7, clientVersionList1.Count);
            Assert.IsTrue(clientVersionList1.Contains("1"));
            Assert.IsTrue(clientVersionList1.Contains("12"));
            Assert.IsTrue(clientVersionList1.Contains("2"));
            Assert.IsTrue(clientVersionList1.Contains("112"));
            Assert.IsTrue(clientVersionList1.Contains("0.0.112"));
            Assert.IsTrue(clientVersionList1.Contains("4"));
            Assert.AreEqual(7, clientVersionList2.Count);
            Assert.IsTrue(clientVersionList2.Contains("5"));
            Assert.IsTrue(clientVersionList2.Contains("jkl"));
            Assert.IsTrue(clientVersionList2.Contains("0.0.1122"));
            Assert.IsTrue(clientVersionList2.Contains("0.1"));
            Assert.IsTrue(clientVersionList2.Contains("12 0.0.112"));
            Assert.IsTrue(clientVersionList2.Contains("0.0.112"));
            Assert.IsTrue(clientVersionList2.Contains("12"));

            Assert.IsTrue(serverInfo1.AreVersionsIntersect(serverInfo2));
            var intersection = serverInfo1.GetVersionIntersection(serverInfo2).ToList();
            Assert.AreEqual(2, intersection.Count());
            Assert.IsTrue(intersection.Contains("0.0.112"));
            Assert.IsTrue(intersection.Contains("12"));

        }
    }
}