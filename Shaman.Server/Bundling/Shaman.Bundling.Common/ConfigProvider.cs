using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Shaman.Bundling.Common
{
    public interface IBundleSettingsProvider
    {
        Dictionary<string, string> GetSettings();
    }
    


}