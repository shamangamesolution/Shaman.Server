using NUnit.Framework;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Tests
{
    [TestFixture]
    public class ChangesContainerTest
    {
        protected ISerializer serializer;

        [SetUp]
        public void Setup()
        {
            serializer = new BinarySerializer();
        }
        
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void SerializationTest()
        {
            var changeSet = new ChangeSet();

            changeSet.TrackChange(1,1,1);
            changeSet.TrackChange(2,2,1);

            changeSet.TrackChange(1,1,(int?)1);
            changeSet.TrackChange(2,2,(int?)1);

            changeSet.TrackChange(1,1,(byte)1);
            changeSet.TrackChange(2,2,(byte)1);

            changeSet.TrackChange(1,1,(byte?)1);
            changeSet.TrackChange(2,3,(byte?)1);

            changeSet.TrackChange(1,1,(float?)1);
            changeSet.TrackChange(2,2,(float?)1);

            
            var newChangeSet = serializer.DeserializeAs<ChangeSet>(serializer.Serialize(changeSet));
            
        }
    }
}