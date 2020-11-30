using System.Dynamic;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Meta;

namespace Shaman.Contract.Bundle
{
    public interface IShamanComponents
    {
        IShamanLogger Logger { get; }
        IBundleConfig Config { get; }
        IMetaProvider MetaProvider { get; }
    }
}