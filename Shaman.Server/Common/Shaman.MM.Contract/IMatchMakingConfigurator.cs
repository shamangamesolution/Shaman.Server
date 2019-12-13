using System.Collections.Generic;

namespace Shaman.MM.Contract
{
    public interface IMatchMakingConfigurator
    {
        void AddMatchMakingGroup(Dictionary<byte, object> measures);
        
        void AddRequiredProperty(byte requiredMatchMakingProperty);
    }
}