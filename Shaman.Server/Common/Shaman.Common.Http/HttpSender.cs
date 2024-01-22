using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Common.Http
{
    public class HttpSender : IRequestSender
    {
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;
        private readonly HttpClient _client;
        
        public HttpSender(IShamanLogger logger, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
            
            //TODO inject this stuff via dep!!!
            // Create an HttpClientHandler object and set to use default credentials
            HttpClientHandler handler = new HttpClientHandler();
            // Set custom server validation callback
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

            _client = HttpClientFactory.Create(handler);
            _client.Timeout = new TimeSpan(0, 0, 15);
        }

        public async Task<T> SendRequest<T>(string serviceUrl, HttpRequestBase request)
            where T : HttpResponseBase, new()
        {
            // _logger.Error($"piiimpa! {serviceUrl}");
            var responseObject = new T();
            var stopwatch = Stopwatch.StartNew();
            var requestUri = $"{serviceUrl}/{request.EndPoint}";

            try
            {
                // Disabling certificate validation can expose you to a man-in-the-middle attack
                // which may allow your encrypted message to be read by an attacker
                // https://stackoverflow.com/a/14907718/740639

                
                if (string.IsNullOrWhiteSpace(serviceUrl))
                    throw new Exception($"Base Url address is empty for {request.GetType()}");

                if (string.IsNullOrWhiteSpace(request.EndPoint))
                    throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");

                var byteContent = new ByteArrayContent(_serializer.Serialize(request));

                using (var message = await _client.PostAsync(requestUri, byteContent))
                {
                    if (!message.IsSuccessStatusCode)
                    {
                        _logger.Error($"SendRequest {request.GetType()} to {requestUri} error ({stopwatch.ElapsedMilliseconds}ms): {message.StatusCode}");

                        responseObject.ResultCode = ResultCode.SendRequestError;
                    }
                    else
                    {
                        using(var contentStream = await message.Content.ReadAsStreamAsync())
                            responseObject = _serializer.DeserializeAs<T>(contentStream);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"SendRequest {request.GetType()}  to {requestUri} error ({stopwatch.ElapsedMilliseconds}ms): {e}");
                responseObject.ResultCode = ResultCode.SendRequestError;
            }

            return responseObject;
        }

        public async Task SendRequest<T>(string serviceUrl, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
        {
            var responseObject = await SendRequest<T>(serviceUrl, request);
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
