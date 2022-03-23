using System.Collections.Generic;

namespace Shaman.Bundling.Common
{
    public interface IBundleSettingsProvider
    {
        /// <summary>
        /// Get settings list
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetSettings();
    }
    


}