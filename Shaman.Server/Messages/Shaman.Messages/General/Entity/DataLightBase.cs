using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Messages.General.Entity
{
    public abstract class DataLightBase : EntityBase
    {
        public int Index { get; set; }
    }
}