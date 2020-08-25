using NUnit.Framework;
using Shaman.Bundling.Common;
using Shaman.Contract.MM;

namespace Shaman.ServerSharedUtilities.Tests
{
    public class BundleHelperTests
    {
        [Test]
        [Ignore("manual testing(need granting access to download)")]
        public void Test1()
        {
            var uri = "https://localhost/mm.zip";
            BundleHelper.LoadTypeFromBundle<IMmResolver>(uri);
        }
    }
}