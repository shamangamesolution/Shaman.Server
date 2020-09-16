using System;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Serialization.Messages.Http;

namespace Shaman.MM.Tests.Fakes
{
    public class FakeSender : IRequestSender
    {        
        public async Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request) where T : HttpResponseBase, new()
        {
            return new T();
        }

        public async Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
        {
            throw new NotImplementedException();
        }
    }
}