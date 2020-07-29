using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Common.Server.Senders
{
    public class HttpSender: IRequestSender
    {
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;

        static HttpSender()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
        }
        
        public HttpSender(IShamanLogger logger, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }
        
        public async Task<T> SendRequest<T>(string serviceUrl, HttpRequestBase request)
            where T : HttpResponseBase, new()
        {
            var responseObject = new T();
            var stopwatch = Stopwatch.StartNew();
            var requestUri = $"{serviceUrl}/{request.EndPoint}";

            try
            {
                if (string.IsNullOrWhiteSpace(serviceUrl))
                    throw new Exception($"Base Url address is empty for {request.GetType()}");                   
                    
                if (string.IsNullOrWhiteSpace(request.EndPoint))
                    throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");
                
                using (var client = HttpClientFactory.Create())
                {
                    client.Timeout = new TimeSpan(0, 0, 15);
                    
                    var byteContent = new ByteArrayContent(_serializer.Serialize(request));

                    using (var message = await client.PostAsync(requestUri, byteContent))
                    {
                        if (!message.IsSuccessStatusCode)
                        {
                            _logger.Error($"SendRequest {request.GetType()} to {requestUri} error ({stopwatch.ElapsedMilliseconds}ms): {message.StatusCode}");                            

                            responseObject.ResultCode = ResultCode.SendRequestError;
                        }
                        else
                        {
                            var contentStream = await message.Content.ReadAsStreamAsync();
                            responseObject = _serializer.DeserializeAs<T>(contentStream);
                        }
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