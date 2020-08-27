using System.Collections.Generic;

namespace Shaman.Contract.Bundle
{
    public interface IBundleConfig
    {
        string GetValueOrNull(string key);
    }
}