using System;
using System.Threading.Tasks;

namespace Shaman.ServerSharedUtilities.StatSenders
{
    public interface IServerStatsSender
    {
        Task SendEvent(string eventName, object item);
        void Initialize(string apiKey);
    }
}