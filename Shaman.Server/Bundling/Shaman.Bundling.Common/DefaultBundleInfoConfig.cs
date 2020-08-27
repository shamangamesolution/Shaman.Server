namespace Shaman.Bundling.Common
{
    public interface IDefaultBundleInfoConfig
    {
        string BundleUri { get; set; }
        bool ToOverwriteExisting { get; set; }
    }
    
    public class DefaultBundleInfoConfig : IDefaultBundleInfoConfig
    {
        public string BundleUri { get; set; }
        public bool ToOverwriteExisting { get; set; }

        public DefaultBundleInfoConfig(string bundleUri, bool toOverwriteExisting)
        {
            BundleUri = bundleUri;
            ToOverwriteExisting = toOverwriteExisting;
        }
    }
}