using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Server.Senders
{
    public class HttpWebRequestSender : IRequestSender
    {
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;

        static HttpWebRequestSender()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
        }

        public HttpWebRequestSender(IShamanLogger logger, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }

        public async Task<T> SendRequest<T>(string serviceUri, HttpRequestBase request)
            where T : HttpResponseBase, new()
        {
            T responseObject = new T();
            var stopwatch = Stopwatch.StartNew();
            var requestUri = $"{serviceUri}/{request.EndPoint}";

            try
            {
                if (string.IsNullOrWhiteSpace(serviceUri))
                    throw new Exception($"Base Url address is empty for {request.GetType()}");

                if (string.IsNullOrWhiteSpace(request.EndPoint))
                    throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");

                var httpWebRequest = WebRequest.CreateHttp(requestUri);
                httpWebRequest.Method = WebRequestMethods.Http.Post;
                httpWebRequest.Timeout = 15000;

                using (var requestStream = httpWebRequest.GetRequestStream())
                    _serializer.Serialize(request, requestStream);


                using (var httpWebResponse = (HttpWebResponse) await httpWebRequest.GetResponseAsync())
                {
                    if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.Error(
                            $"SendRequest {request.GetType()} to {requestUri} error ({stopwatch.ElapsedMilliseconds}ms): {httpWebResponse.StatusCode}");

                        responseObject.ResultCode = ResultCode.SendRequestError;
                    }
                    else
                    {
                        responseObject = _serializer.DeserializeAs<T>(httpWebResponse.GetResponseStream());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(
                    $"SendRequest {request.GetType()}  to {requestUri} error ({stopwatch.ElapsedMilliseconds}ms): {e}");
                responseObject.ResultCode = ResultCode.SendRequestError;
            }

            return responseObject;
        }

        public async Task SendRequest<T>(string serviceUri, HttpRequestBase request, Action<T> callback)
            where T : HttpResponseBase, new()
        {
            var responseObject = await SendRequest<T>(serviceUri, request);
            try
            {
                callback(responseObject);
            }
            catch (Exception ex)
            {
                _logger.Error($"SendRequest callback error: {ex}");
                throw;
            }
        }
    }
}