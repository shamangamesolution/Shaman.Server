using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Server.Senders
{
    public class HttpSender : IRequestSender
    {
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;
        
        public HttpSender(IShamanLogger logger, ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }
        
        public async Task<T> SendRequest<T>(string url, RequestBase request)
            where T : ResponseBase, new()
        {
            T responseObject = new T();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 15);
                    
                    ByteArrayContent byteContent = new ByteArrayContent(_serializer.Serialize(request));
                   
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

                    if (string.IsNullOrWhiteSpace(url))
                        throw new Exception($"Base Url address is empty for {request.GetType()}");                   
                    
                    if (string.IsNullOrWhiteSpace(request.EndPoint))
                        throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");

                    var uri = $"{url}/{request.EndPoint}";
                    using (var message = await client.PostAsync(uri, byteContent))
                    {
                        if (!message.IsSuccessStatusCode)
                        {
                            _logger.Error($"SendRequest {request.GetType()} to {url} error: {message.Content.ReadAsStringAsync().Result}");                            
                            responseObject.ResultCode = ResultCode.SendRequestError;
                        }
                        else
                        {
                            var response = message.Content.ReadAsByteArrayAsync().Result;
                            responseObject = _serializer.DeserializeAs<T>(response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"SendRequest {request.GetType()}  to {url} error: {e}");
                responseObject.ResultCode = ResultCode.SendRequestError;                
            }
            
            return responseObject;
        }

        public async Task SendRequest<T>(string url, RequestBase request, Action<T> callback) where T : ResponseBase, new()
        {
            T responseObject = new T();
            var requestUrl = $"{url}/{request.EndPoint}";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 15);

                    ByteArrayContent byteContent = new ByteArrayContent(_serializer.Serialize(request));
                   
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

                    if (string.IsNullOrWhiteSpace(url))
                        throw new Exception($"Url address is empty");                   
                    if (string.IsNullOrWhiteSpace(request.EndPoint))
                        throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");

                    using (var message = await client.PostAsync(requestUrl, byteContent))
                    {
                        if (!message.IsSuccessStatusCode)
                        {
                            _logger.Error($"SendRequest {request.GetType()} error: {message.Content.ReadAsStringAsync().Result}");                            
                            responseObject.ResultCode = ResultCode.SendRequestError;
                            responseObject.Message = message.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            var response = message.Content.ReadAsByteArrayAsync().Result;
                            responseObject = _serializer.DeserializeAs<T>(response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"SendRequest {request.GetType()} error: {e}");
                responseObject.ResultCode = ResultCode.SendRequestError;
            }

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
        
//        public async Task<TResponse> SendRequest<TRequest, TResponse>(string backEndAddress, TRequest request) 
//            where TRequest : BasicRequest
//            where TResponse :BasicResponse, new()
//        {
//            request.Token = string.Empty;
//            var backendUrl = backEndAddress + $"/{request.EndPoint}";
//            var stopwatch = Stopwatch.StartNew();
//            try
//            {
//                using (var client = new HttpClient())
//                {
//                    client.Timeout = new TimeSpan(0, 0, 15);
//                    var byteContent = new ByteArrayContent(request.SerializeNew());
//
//                    if (string.IsNullOrWhiteSpace(backEndAddress))
//                    {
//                        var message =
//                            $"BackEnd url is not available for request {request.EndPoint}:\n{System.Environment.StackTrace}";
//                        _logger.Error(message);
//                        var response = new TResponse();
//                        response.SetError(message);
//                        return response;
//                    }
//
//                    using (var message = await client.PostAsync(backendUrl, byteContent))
//                    {
//                        var response = message.Content.ReadAsByteArrayAsync().Result;
//                        var responseObject = response.DeserializeTo<TResponse>();
//                        responseObject.BackendAddress = backEndAddress;
//                        return responseObject;
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                var message =
//                    $"Send request {request.OperationSubCode} to '{backendUrl}' error (duration {stopwatch.ElapsedMilliseconds}ms): {e}";
//                _logger.Error(message);
//                var response = new TResponse();
//                response.SetError(message);
//                return response;
//            }
//        }
//
//        public async Task SendRequest<TRequest, TResponse, TContext>(string backEndAddress, TRequest request, Action<TRequest, TResponse, TContext> callback, TContext ctx = null) 
//            where TRequest : BasicRequest 
//            where TResponse : BasicResponse, new()
//            where TContext : class
//        {
//            request.Token = string.Empty;
//            var backendUrl = backEndAddress + $"/{request.EndPoint}";
//            var stopwatch = Stopwatch.StartNew();
//            try
//            {
//                using (var client = new HttpClient())
//                {
//                    client.Timeout = new TimeSpan(0, 0, 15);
//                    var byteContent = new ByteArrayContent(request.SerializeNew());
//
//                    if (string.IsNullOrWhiteSpace(backEndAddress))
//                    {
//                        var message =
//                            $"BackEnd url is not available for request {request.EndPoint}:\n{System.Environment.StackTrace}";
//                        _logger.Error(message);
//                        var response = new TResponse();
//                        response.SetError(message);
//                        callback?.Invoke(request, response, ctx);
//                    }
//
//                    using (var message = await client.PostAsync(backendUrl, byteContent))
//                    {
//                        var response = message.Content.ReadAsByteArrayAsync().Result;
//                        var responseObject = response.DeserializeTo<TResponse>();
//                        responseObject.BackendAddress = backEndAddress;
//                        callback?.Invoke(request, responseObject, ctx);
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                var message =
//                    $"Send request {request.OperationSubCode} to '{backendUrl}' error (duration {stopwatch.ElapsedMilliseconds}ms): {e}";
//                _logger.Error(message);
//                var response = new TResponse();
//                response.SetError(message);
//                callback?.Invoke(request, response, ctx);
//            }
//        }
//
//        public Task SendRequest(string backEndAddress, SaOperationRequest request, Action<SaOperationResponse> callback)
//        {
//            throw new NotImplementedException();
//        }

    }
}