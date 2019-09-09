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
        private ISerializerFactory _serializerFactory;
        [SetUp]
        public void Setup()
        {
            _serializerFactory = new SerializerFactory(_logger);
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