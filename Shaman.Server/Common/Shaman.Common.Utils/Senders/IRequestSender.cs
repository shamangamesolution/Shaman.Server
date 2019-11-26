using System;
using System.Threading.Tasks;
using Shaman.Common.Utils.Messages;

namespace Shaman.Common.Utils.Senders
{
    public interface IRequestSender
    {
        Task<T> SendRequest<T>(string url, RequestBase request) where T: ResponseBase, new();
        
        Task SendRequest<T>(string url, RequestBase request, Action<T> callback) where T: ResponseBase, new();

    }
}