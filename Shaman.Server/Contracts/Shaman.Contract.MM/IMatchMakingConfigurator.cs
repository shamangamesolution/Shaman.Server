using System.Collections.Generic;

namespace Shaman.Contract.MM
{
    public interface IMatchMakingConfigurator
    {
        void AddMatchMakingGroup(Dictionary<byte, object> measures);
        
        void AddRequiredProperty(byte requiredMatchMakingProperty);
    }
}