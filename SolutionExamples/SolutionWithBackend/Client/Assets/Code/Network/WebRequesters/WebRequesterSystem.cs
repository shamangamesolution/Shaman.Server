using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Code.Network.WebRequesters
{
    public class WebRequesterSystem : IWebRequester
    {
        private readonly IWebRequesterConfigProvider _webRequesterConfigProvider;

        public WebRequesterSystem(IWebRequesterConfigProvider webRequesterConfigProvider)
        {
            _webRequesterConfigProvider = webRequesterConfigProvider;
        }

        public void ThrowIfNotCreated(string url)
        {
            var request = CreateRequestGet(url);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
                throw new ArgumentNullException(nameof(responseStream));

            var streamReader = new StreamReader(responseStream);

            streamReader.ReadToEnd();
        }

        void IWebRequester.Initialize()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;
            WebRequest.DefaultWebProxy = null;
        }

        async Task<string> IWebRequester.GetStringAsync(string url)
        {
            try
            {
                var request = CreateRequestGet(url);
                var response = await request.GetResponseAsync();

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                        throw new NullReferenceException(nameof(responseStream));

                    using (var streamReader = new StreamReader(responseStream))
                    {
                        return await streamReader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception exception)
            {
                if (_webRequesterConfigProvider.EnableThrowingExceptions)
                    throw new Exception($"Exception occured during request to '{url}': {exception}");

                return null;
            }
        }

        async Task<RequestResult> IWebRequester.PostAsync(string url, byte[] postData)
        {
            var webRequest = CreateRequestPost(url, postData.LongLength);

            try
            {
                var bytes = await ReadResponseBytesAsync(postData, webRequest);
                
                return new RequestResult
                {
                    IsSuccess = true,
                    Data = bytes
                };
            }
            catch (Exception exception)
            {
                if (_webRequesterConfigProvider.EnableThrowingExceptions)
                    throw new Exception($"Exception occured during request to '{url}': {exception}");

                return new RequestResult
                {
                    IsSuccess = false,
                    Exception = exception
                };
            }
        }

        private HttpWebRequest CreateRequestPost(string url, long contentLength)
        {
            var webRequest = WebRequest.CreateHttp(url);

            webRequest.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            webRequest.SendChunked = false;
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = _webRequesterConfigProvider.RequestTimeoutMilliseconds;
            webRequest.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
            webRequest.ContentLength = contentLength;

            return webRequest;
        }

        private HttpWebRequest CreateRequestGet(string url)
        {
            var webRequest = WebRequest.CreateHttp(url);

            webRequest.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            webRequest.SendChunked = false;
            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.Timeout = _webRequesterConfigProvider.RequestTimeoutMilliseconds;

            return webRequest;
        }

        private async Task<byte[]> ReadResponseBytesAsync(byte[] postData, HttpWebRequest webRequest)
        {
            byte[] bytes = null;

            Stream requestStream = null;

            try
            {
                var requestStreamTask = webRequest.GetRequestStreamAsync();

                await Task.WhenAny(requestStreamTask, Task.Delay(_webRequesterConfigProvider.RequestStreamTimeoutMilliseconds));

                if (!requestStreamTask.IsCompleted)
                    throw new TimeoutException();

                requestStream = requestStreamTask.Result;

                await requestStream.WriteAsync(postData, 0, postData.Length);

                using (var response = await webRequest.GetResponseAsync())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new NullReferenceException(nameof(responseStream));

                        using (var memoryStream = new MemoryStream())
                        {
                            await responseStream.CopyToAsync(memoryStream);
                            bytes = memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (_webRequesterConfigProvider.EnableThrowingExceptions)
                    throw;
            }
            finally
            {
                requestStream?.Dispose();
            }

            return bytes;
        }
    }
}