using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages
{
    public class TestRoomEvent : EventBase
    {
        public override bool IsReliable => true;
        public override bool IsBroadcasted => true;

        public bool TestBool { get; set; }
        public int TestInt { get; set; }
        public float TestFloat { get; set; }
        
        public List<int> TestList { get; set; }
        
        public TestRoomEvent(bool testBool, int testInt, float testFloat, List<int> testList) 
            : base(0)
        {
            this.TestBool = testBool;
            this.TestInt = testInt;
            this.TestFloat = testFloat;
            TestList = testList;
        }

        public TestRoomEvent() : base(Messages.ShamanOperationCode.Test)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(TestList);
            typeWriter.Write(TestBool);
            typeWriter.Write(TestInt);
            typeWriter.Write(TestFloat);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            TestList = typeReader.ReadListOfInt();
            TestBool = typeReader.ReadBool();
            TestInt = typeReader.ReadInt();
            TestFloat = typeReader.ReadFloat();
        }
    }
}