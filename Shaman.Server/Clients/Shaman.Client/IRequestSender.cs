using System;
using System.Threading.Tasks;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Client
{
    public interface IRequestSender
    {
        Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request) where T: HttpResponseBase, new();
        Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback) where T: HttpResponseBase, new();
    }
}