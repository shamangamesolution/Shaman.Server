using System;
using System.Threading.Tasks;
using Code.Common;
using Code.Network.WebRequesters;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using UnityEngine;

namespace Code.Network
{
	public class UnityRequestSender : MonoBehaviour, IRequestSender
	{
		[Inject]
		public IWebRequester webRequester { get; set; }
	
		[Inject]
		public IShamanLogger logger { get; set; }
	
		[Inject]
		public ISerializer serializerFactory { get; set; }
		
		public async Task<T> SendRequest<T>(string url, HttpRequestBase request) where T : HttpResponseBase, new()
		{
			try
			{
				if (string.IsNullOrEmpty(request.EndPoint))
				{
					logger.Error("Endpoint is not set for this request");
					return new T() {ResultCode = ResultCode.SendRequestError, Message = "Endpoint is not set for this request"};
				}
				string serverUrl = url + "/" + request.EndPoint;
				
				var result = await webRequester.PostAsync(serverUrl, serializerFactory.Serialize(request));
				if (result.IsSuccess)
					return serializerFactory.DeserializeAs<T>(result.Data);
				else
					throw result.Exception;
			}
			catch (Exception e)
			{
				logger.Error(string.Format("Backend response deserializing error ({1}): {0}", e, request));
				return new T() {ResultCode = ResultCode.SendRequestError, Message = e.Message};
			}
		}

		public Task SendRequest<T>(string url, HttpRequestBase request, Action<T> callback) where T : HttpResponseBase, new()
		{
			throw new NotImplementedException();
		}
	}
}


