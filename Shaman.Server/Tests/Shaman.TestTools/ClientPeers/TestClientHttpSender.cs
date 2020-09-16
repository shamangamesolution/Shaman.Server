using System;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;
using IRequestSender = Shaman.Client.IRequestSender;

namespace Shaman.TestTools.ClientPeers
{
    class TestClientHttpSender:IRequestSender
    {
        private readonly HttpSender _httpSender;

        public TestClientHttpSender(IShamanLogger logger, ISerializer serializer)
        {
            _httpSender = new HttpSender(logger,serializer);
        }

        public Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request) where T : HttpResponseBase, new()
        {
            return _httpSender.SendRequest<T>(serviceUri, request);
        }

        public Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
        {
            return _httpSender.SendRequest<T>(serviceUri, request, callback);
        }
        
    }
}