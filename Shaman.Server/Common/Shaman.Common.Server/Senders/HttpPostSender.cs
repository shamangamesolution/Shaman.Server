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
        private IShamanLogger _logger;
        private ISerializerFactory _serializerFactory;
        
        public HttpSender(IShamanLogger logger, ISerializerFactory serializerFactory)
        {
            _logger = logger;
            _serializerFactory = serializerFactory;
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
                    
                    ByteArrayContent byteContent = new ByteArrayContent(request.Serialize(_serializerFactory));
                   
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

                    if (string.IsNullOrWhiteSpace(url))
                        throw new Exception($"Base Url address is empty for {request.GetType()}");                   
                    
                    if (string.IsNullOrWhiteSpace(request.EndPoint))
                        throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");    
                    
                    using (var message = await client.PostAsync($"{url}/{request.EndPoint}", byteContent))
                    {
                        if (!message.IsSuccessStatusCode)
                        {
                            _logger.Error($"SendRequest {request.GetType()} error: {message.Content.ReadAsStringAsync().Result}");                            
                            responseObject.ResultCode = ResultCode.SendRequestError;
                        }
                        else
                        {
                            var response = message.Content.ReadAsByteArrayAsync().Result;
                            responseObject = MessageBase.DeserializeAs<T>(_serializerFactory, response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"SendRequest {request.GetType()} error: {e}");
                responseObject.ResultCode = ResultCode.SendRequestError;                
            }
            
            return responseObject;
        }

        public async Task SendRequest<T>(string url, RequestBase request, Action<T> callback) where T : ResponseBase, new()
        {
            T responseObject = new T();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 15);
                    
                    ByteArrayContent byteContent = new ByteArrayContent(request.Serialize(_serializerFactory));
                   
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

                    if (string.IsNullOrWhiteSpace(url))
                        throw new Exception($"Url address is empty");                   
                    if (string.IsNullOrWhiteSpace(request.EndPoint))
                        throw new Exception($"Request endpoint Url address is empty for {request.GetType()}");  
                    
                    using (var message = await client.PostAsync($"{url}/{request.EndPoint}", byteContent))
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
                            responseObject = MessageBase.DeserializeAs<T>(_serializerFactory, response);
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
    }
}