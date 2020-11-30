using NUnit.Framework;
using Shaman.Messages.General.Entity;
using Shaman.Messages.Helpers;
using Shaman.Serialization;

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
        
        [Test]
        public void SplitTest()
        {
            var changeSet = new ChangeSet();
            
            //30
            changeSet.TrackChange(1,1,1);
            changeSet.TrackChange(2,2,1);

            //30
            changeSet.TrackChange(1,1,(int?)1);
            changeSet.TrackChange(2,2,(int?)1);
            changeSet.TrackChange(3,2,(int?)1);

            //20
            changeSet.TrackChange(1,1,(byte)1);
            changeSet.TrackChange(2,2,(byte)1);
            changeSet.TrackChange(2,3,(byte)1);

            //20
            changeSet.TrackChange(1,1,(byte?)1);
            changeSet.TrackChange(2,3,(byte?)1);
            changeSet.TrackChange(2,3,(byte?)1);

            //30
            changeSet.TrackChange(1,1,(float?)1);
            changeSet.TrackChange(2,2,(float?)1);

            var size = changeSet.GetSizeInBytes();
            
            Assert.AreEqual(120, size);

            var splited = UpdateInfoHelper.Split(changeSet, 5);
            Assert.AreEqual(5, splited.Count);

            var updateInfo = new UpdatedInfo(changeSet, 3);
            var splitedUpdateInfo = UpdateInfoHelper.Split(updateInfo, 60);
            Assert.AreEqual(3, splitedUpdateInfo.Count);

            updateInfo = new UpdatedInfo(changeSet, 3);
            splitedUpdateInfo = UpdateInfoHelper.Split(updateInfo, 80);
            Assert.AreEqual(2, splitedUpdateInfo.Count);
            
            updateInfo = new UpdatedInfo(changeSet, 3);
            splitedUpdateInfo = UpdateInfoHelper.Split(updateInfo, 300);
            Assert.AreEqual(1, splitedUpdateInfo.Count);
        }
    }
}