using System;
using System.Collections.Generic;
using System.Linq;
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
            ChildList = new List<TestChildEntity>();
            ChildDictionary = new EntityDictionary<TestChildEntity>();
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
        private MongoDbConnector _connector;
        private TestEntity _first = new TestEntity(1, 4, "test1");
        private TestEntity _second = new TestEntity(2, 0, "test2")
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
        private TestEntity _third = new TestEntity(3, int.MaxValue, null);

        [SetUp]
        public async Task SetUp()
        {
            var settings = new CustomMongoClientSettings("mongodb://localhost", "testdb");
            var mapperFactory = new DefaultMongoDbMapperFactory();
            _connector = new MongoDbConnector(settings, mapperFactory);
            _connector.Connect();
            await TearDown();
        }

        [TearDown]
        public async Task TearDown()
        {
            await RemoveAll();
        }
        
        
        private async Task<TestEntity> Get(int id)
        {
            return await _connector.Get<TestEntity>(id);
        }


        public async Task RemoveAll()
        {
            await _connector.Remove<TestEntity>(1);
            await _connector.Remove<TestEntity>(e => e.Id == 2 || e.Id == 3);
        }
        
        [Test]
        public async Task CreateTests()
        {
            await _connector.Create(_first);
            await _connector.Create(_second);
            await _connector.Create(_third);
        }

        [Test]
        public async Task CreateGetRemoveTests()
        {
            await CreateTests();
            var receivedFirst = await Get(_first.Id);
            var receivedSecond = await Get(_second.Id);
            var receivedThird = await Get(_third.Id);
            
            //asserts
            var jsonedFirst = JsonConvert.SerializeObject(_first);
            var jsonedReceivedFirst = JsonConvert.SerializeObject(receivedFirst);
            
            var jsonedSecond = JsonConvert.SerializeObject(_second);
            var jsonedReceivedSecond = JsonConvert.SerializeObject(receivedSecond);
            
            var jsonedThird = JsonConvert.SerializeObject(_third);
            var jsonedReceivedThird = JsonConvert.SerializeObject(receivedThird);
            
            
            Assert.AreEqual(jsonedFirst, jsonedReceivedFirst);
            Assert.AreEqual(jsonedSecond, jsonedReceivedSecond);
            Assert.AreEqual(jsonedThird, jsonedReceivedThird);

            await RemoveAll();
            
            receivedFirst = await Get(_first.Id);
            receivedSecond = await Get(_second.Id);
            receivedThird = await Get(_third.Id);
            
            Assert.IsNull(receivedFirst);
            Assert.IsNull(receivedSecond);
            Assert.IsNull(receivedThird);
            
            //create again
            await CreateTests();

            //get bunch
            var result = await _connector.Get<TestEntity>(e => e.Id == 2 || e.Id == 3);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetFieldsTests()
        {
            await CreateTests();

            var result = await _connector.GetFields<TestEntity>(1)
                .Include(x => x.StringField)
                .GetOne();
            
            Assert.AreEqual("test1", result.StringField);
            Assert.AreEqual(0, result.IntField);
            
            result = await _connector.GetFields<TestEntity>(1)
                .Include(x => x.IntField)
                .GetOne();
            
            Assert.IsNull(result.StringField);
            Assert.AreEqual(4, result.IntField);
        }

        [Test]
        public async Task PushPullTests()
        {
            await CreateTests();

            await _connector.UpdateWhere<TestEntity>(i => i.Id == 1)
                .Push<TestChildEntity>(e => e.ChildList, new TestChildEntity(5, true, 3.3f))
                .Push<TestChildEntity>(e => e.ChildDictionary, new TestChildEntity(6, false, 2.1f))
                .Update();
            
            var receivedFirst = await Get(_first.Id);
            Assert.AreEqual(1, receivedFirst.ChildList.Count);
            Assert.AreEqual(1, receivedFirst.ChildDictionary.Count);
            
            await _connector.UpdateWhere<TestEntity>(i => i.Id == 1)
                .Push<TestChildEntity>(e => e.ChildList, new TestChildEntity(7, false, 1.3f))
                .Push<TestChildEntity>(e => e.ChildDictionary, new TestChildEntity(8, true, 2.7f))
                .Update();
            
            receivedFirst = await Get(_first.Id);
            Assert.AreEqual(2, receivedFirst.ChildList.Count);
            Assert.AreEqual(2, receivedFirst.ChildDictionary.Count);
            
            await _connector.UpdateWhere<TestEntity>(i => i.Id == 1)
                .Pull<TestChildEntity>(e => e.ChildList, e=>e.Id == 5)
                .Pull<TestChildEntity>(e => e.ChildDictionary, e => e.Id == 6)
                .Update();
            
            receivedFirst = await Get(_first.Id);
            Assert.AreEqual(1, receivedFirst.ChildList.Count);
            Assert.AreEqual(1, receivedFirst.ChildDictionary.Count);
            Assert.AreEqual(7, receivedFirst.ChildList.First().Id);
            Assert.AreEqual(8, receivedFirst.ChildDictionary.First().Id);
        }
        
        [Test]
        public async Task CreateUpdateRemoveTests()
        {
            await CreateTests();
            
            var fieldsProvider = new MongoDbFieldProvider<TestEntity>();
            fieldsProvider.Add(x => x.IntField, 10);
            fieldsProvider.Add(x => x.StringField, "update123");
            fieldsProvider.Add(x => x.ChildList[0].FloatField, 123.76f);
            fieldsProvider.Add(x => x.ChildList[1].FloatField, 76.123f);
            fieldsProvider.Add(x => x.ChildDictionary[-1].FloatField, 1.01f);

            await _connector.Update(i => i.Id == _second.Id && i.ChildDictionary.Any(d => d.Id == 4), fieldsProvider);
            
            var receivedFirst = await Get(_second.Id);
            Assert.AreEqual(10, receivedFirst.IntField);
            Assert.AreEqual("update123", receivedFirst.StringField);
            Assert.AreEqual(123.76f, receivedFirst.ChildList[0].FloatField);
            Assert.AreEqual(76.123f, receivedFirst.ChildList[1].FloatField);
            Assert.AreEqual(1.01f, receivedFirst.ChildDictionary[4].FloatField);
        }
        
        [Test]
        public async Task CreateUpdateRemoveTests2()
        {
            await CreateTests();
            
            await _connector.UpdateWhere<TestEntity>(i => i.Id == _second.Id && i.ChildDictionary.Any(d => d.Id == 4))
                .Set(x => x.IntField, 10)
                .Set(x => x.StringField, "update123")
                .Set(x => x.ChildList[0].FloatField, 123.76f)
                .Set(x => x.ChildList[1].FloatField, 76.123f)
                .Set(x => x.ChildDictionary[-1].FloatField, 1.01f)
                .Update();
            var receivedFirst = await Get(_second.Id);
            Assert.AreEqual(10, receivedFirst.IntField);
            Assert.AreEqual("update123", receivedFirst.StringField);
            Assert.AreEqual(123.76f, receivedFirst.ChildList[0].FloatField);
            Assert.AreEqual(76.123f, receivedFirst.ChildList[1].FloatField);
            Assert.AreEqual(1.01f, receivedFirst.ChildDictionary[4].FloatField);

        }
        

        
    }
}