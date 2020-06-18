using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages;

namespace Shaman.DAL.MongoDb.Tests
{
    public class TestChildEntity : EntityBase
    {
        public TestChildEntity(int id, bool boolField, float floatField)
        {
            Id = id;
            BoolField = boolField;
            FloatField = floatField;
        }

        public bool BoolField { get; set; }
        public float FloatField { get; set; }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            throw new NotImplementedException();
        }
    }
    
    public class TestEntity : EntityBase
    {
        public TestEntity(int id, int intField, string stringField)
        {
            Id = id;
            IntField = intField;
            StringField = stringField;
        }

        public int IntField { get; set; }
        public string StringField { get; set; }
        
        public List<TestChildEntity> ChildList { get; set; }
        public EntityDictionary<TestChildEntity> ChildDictionary { get; set; }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            throw new System.NotImplementedException();
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            throw new System.NotImplementedException();
        }
    }
    
    [TestFixture]
    public class MongoDbDefaultTests
    {
        private MongoDbRepository _repo;
        private TestEntity first = new TestEntity(1, 4, "test1");
        private TestEntity second = new TestEntity(2, 0, "test2")
        {
            ChildList = new List<TestChildEntity>
            {
                new TestChildEntity(1, false, 4.6f),
                new TestChildEntity(2, true, 0f),
            },
            ChildDictionary = new EntityDictionary<TestChildEntity>()
            {
                new TestChildEntity(3, false, 1000f),
                new TestChildEntity(4, true, 3001.6785f),
            }
        };
        private TestEntity third = new TestEntity(3, int.MaxValue, null);

        [SetUp]
        public void SetUp()
        {
            var settings = new CustomMongoClientSettings("testdb", new MongoClientSettings
            {
                MaxConnectionPoolSize = 1000,
                Server = new MongoServerAddress("localhost")
            });
            var mapperFactory = new DefaultMongoDbMapperFactory();
            _repo = new MongoDbRepository(settings, mapperFactory);
            _repo.Connect();
        }

        [TearDown]
        public void TearDown()
        {
            
        }
        
        
        private async Task<TestEntity> Get(int id)
        {
            return await _repo.Get<TestEntity>(id);
        }


        public async Task RemoveAll()
        {
            await _repo.Remove<TestEntity>(1);
            await _repo.Remove<TestEntity>(e => e.Id == 2 || e.Id == 3);
        }
        
        [Test]
        public async Task CreateTests()
        {
            await _repo.Create(first);
            await _repo.Create(second);
            await _repo.Create(third);
        }

        [Test]
        public async Task CreateGetRemoveTests()
        {
            await CreateTests();
            var receivedFirst = await Get(first.Id);
            var receivedSecond = await Get(second.Id);
            var receivedThird = await Get(third.Id);
            
            //asserts
            var jsonedFirst = JsonConvert.SerializeObject(first);
            var jsonedReceivedFirst = JsonConvert.SerializeObject(receivedFirst);
            
            var jsonedSecond = JsonConvert.SerializeObject(second);
            var jsonedReceivedSecond = JsonConvert.SerializeObject(receivedSecond);
            
            var jsonedThird = JsonConvert.SerializeObject(third);
            var jsonedReceivedThird = JsonConvert.SerializeObject(receivedThird);
            
            
            Assert.AreEqual(jsonedFirst, jsonedReceivedFirst);
            Assert.AreEqual(jsonedSecond, jsonedReceivedSecond);
            Assert.AreEqual(jsonedThird, jsonedReceivedThird);

            await RemoveAll();
            
            receivedFirst = await Get(first.Id);
            receivedSecond = await Get(second.Id);
            receivedThird = await Get(third.Id);
            
            Assert.IsNull(receivedFirst);
            Assert.IsNull(receivedSecond);
            Assert.IsNull(receivedThird);
            
            //create again
            await CreateTests();

            //get bunch
            var result = await _repo.Get<TestEntity>(e => e.Id == 2 || e.Id == 3);
            Assert.AreEqual(2, result.Count);
            
            //remove all
            await RemoveAll();
        }

        [Test]
        public async Task UpdateTests()
        {
            await CreateTests();
            first.IntField = 10;
            
            await _repo.Update<TestEntity, int>(first.Id, x => x.IntField, 10);
            
            var receivedFirst = await Get(first.Id);
            Assert.AreEqual(10, receivedFirst.IntField);
            
            await RemoveAll();
        }
        

        
    }
}