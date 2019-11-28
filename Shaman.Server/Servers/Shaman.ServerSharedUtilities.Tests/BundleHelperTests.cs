using NUnit.Framework;
using Shaman.MM.Contract;

namespace Shaman.ServerSharedUtilities.Tests
{
    public class BundleHelperTests
    {
        [Test]
        [Ignore("manual testing")]
        public void Test1()
        {
            BundleHelper.LoadTypeFromBundle<IMmResolver>("https://***REMOVED***/mm.zip");
            Assert.Pass();
        }
    }
}