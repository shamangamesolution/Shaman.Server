using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Router.Controllers
{
    public class WebControllerBase : Controller
    {
        protected readonly IOptions<RouterConfiguration> Config;
        protected readonly IShamanLogger Logger;
        protected readonly IConfigurationRepository ConfigRepo;
        protected readonly ISerializer Serializer;
        
        public WebControllerBase(IConfigurationRepository configRepo, 
            IShamanLogger logger, 
            IOptions<RouterConfiguration> config,
            ISerializer serializer)
        {
            this.Serializer = serializer;
            this.ConfigRepo = configRepo;

            this.Logger = logger;
            this.Config = config;
        }
        
        protected async Task SendRequest(string url, HttpRequestBase request)
        {            
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0,0,60);
                ByteArrayContent byteContent = new ByteArrayContent(Serializer.Serialize(request));
                //await client.PostAsync(url, byteContent);
                using (var message = client.PostAsync(url, byteContent).Result)
                {
                    if (!message.IsSuccessStatusCode)
                        LogError($"Send to {url} error: {message.StatusCode}");
                    
                    return;
                }                
            }
        }
        
        protected void SaySlackBot(string text, string channelId)
        {
            var uri = new Uri("https://slack.com/api/chat.postMessage?token="
                              + Config.Value.SlackBotToken + "&channel=" + channelId + "&text=" + text);

            var httpClient = new HttpClient();
            httpClient.GetAsync(uri);
        }

        protected void LogError(string message)
        {
            Logger.Error("Router.Web",
                $"{ControllerContext.ActionDescriptor.ControllerName}.{ControllerContext.ActionDescriptor.ActionName}",
                message);
        }

        protected void LogInfo(string message)
        {
            Logger.Info("Router.Web",
                $"{ControllerContext.ActionDescriptor.ControllerName}.{ControllerContext.ActionDescriptor.ActionName}",
                message);
        }
        
        protected void LogWarning(string message)
        {
            Logger.Warning("Router.Web",
                $"{ControllerContext.ActionDescriptor.ControllerName}.{ControllerContext.ActionDescriptor.ActionName}",
                message);
        }
    }
}