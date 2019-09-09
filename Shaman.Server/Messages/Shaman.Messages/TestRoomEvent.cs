using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Shaman.Messages
{
    public class TestRoomEvent : EventBase
    {
        public bool TestBool { get; set; }
        public int TestInt { get; set; }
        public float TestFloat { get; set; }
        
        public List<int> TestList { get; set; }
        
        public TestRoomEvent(bool testBool, int testInt, float testFloat, List<int> testList) 
            : base(Messages.CustomOperationCode.Test)
        {
            this.TestBool = testBool;
            this.TestInt = testInt;
            this.TestFloat = testFloat;
            TestList = testList;
        }

        public TestRoomEvent() : base(Messages.CustomOperationCode.Test)
        {
        }

        protected override void SetMessageParameters()
        {
            IsReliable = true;
            IsOrdered = true;
            IsBroadcasted = true;
        }

        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteList(TestList);
            serializer.WriteBool(TestBool);
            serializer.WriteInt(TestInt);
            serializer.WriteFloat(TestFloat);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            TestList = serializer.ReadIntList();
            TestBool = serializer.ReadBool();
            TestInt = serializer.ReadInt();
            TestFloat = serializer.ReadFloat();
        }
    }
}