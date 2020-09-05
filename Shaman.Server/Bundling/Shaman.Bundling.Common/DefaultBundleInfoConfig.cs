namespace Shaman.Bundling.Common
{
    public interface IDefaultBundleInfoConfig
    {
        string BundleUri { get; set; }
        bool ToOverwriteExisting { get; set; }
        string ServerRole { get; set; }
    }
    
    public class DefaultBundleInfoConfig : IDefaultBundleInfoConfig
    {
        public string BundleUri { get; set; }
        public bool ToOverwriteExisting { get; set; }
        //used to add additional subdirectory after budle load
        public string ServerRole { get; set; }
        
        public DefaultBundleInfoConfig(string bundleUri, bool toOverwriteExisting, string serverRole)
        {
            BundleUri = bundleUri;
            ToOverwriteExisting = toOverwriteExisting;
            ServerRole = serverRole;
        }
    }
}