using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages;

namespace Shaman.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        private IShamanLogger _logger = new ConsoleLogger();
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            // none
        }

       
        [Test]
        public void NamesProviderTests()
        {
            
        }
    }
}