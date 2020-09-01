using System;
using System.Collections.Generic;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

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

        protected override void SerializeBody(Serialization.ITypeWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        protected override void DeserializeBody(Serialization.ITypeReader typeReader)
        {
            throw new NotImplementedException();
        }
    }

    
    public class TestEntity : EntityBase
    {
        public string StringId { get; set; }
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
            throw new NotImplementedException();
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            throw new NotImplementedException();
        }
    }
}