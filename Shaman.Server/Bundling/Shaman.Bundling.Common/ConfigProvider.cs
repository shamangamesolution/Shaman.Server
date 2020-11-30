using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

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