using NUnit.Framework;
using Shaman.MM.Contract;

namespace Shaman.ServerSharedUtilities.Tests
{
    public class BundleHelperTests
    {
        [Test]
        [Ignore("manual testing(need granting access to download)")]
        public void Test1()
        {
            var uri = "https://***REMOVED***/mm.zip";
            BundleHelper.LoadTypeFromBundle<IMmResolver>(uri);
        }
    }
}