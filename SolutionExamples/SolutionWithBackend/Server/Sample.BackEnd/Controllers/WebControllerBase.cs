using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.BackEnd.BL;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.BackEnd.Extensions;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Exceptions;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.BackEnd.Controllers
{
    public class WebControllerBase : Controller
    {
        private Stopwatch stopwatchTotal = new Stopwatch();        
        protected IOptions<BackendConfiguration> Config;
        protected IShamanLogger Logger;
        protected ICacher Cacher;
        protected IPlayerStorage PlayerStorage;
        
        protected PlayerBL playerBL = null;

        protected IPlayerRepository PlayerRepo;
        protected ITempRepository TempRepo;
        
        protected string slackBotToken = "xoxb-238082241507-502234738672-JLuH2Dw1NNQpCqXXZiwxfXhe";
        protected string devChannelId = "G6ZJWQ6M7";
        protected ISerializer SerializerFactory;

        protected IStorageContainer StorageContainer;
        
        public WebControllerBase(IPlayerRepository playerRepo, 
            ITempRepository tempRepo,
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config,
            ICacher cacher,
            IPlayerStorage playerStorage,
            ISerializer serializerFactory,
            IStorageContainer storageContainer)
        {
            this.PlayerRepo = playerRepo;
            this.TempRepo = tempRepo;
            this.Logger = logger;
            this.Config = config;
            this.SerializerFactory = serializerFactory;
            this.StorageContainer = storageContainer;
            
            Cacher = cacher;
            PlayerStorage = playerStorage;
        }

        protected async Task UpdatePlayerCalculatedFields(Player player)
        {
            StorageContainer.GetStorage().SetPlayerStaticData(player);
        }

        protected async Task<ActionResult> ProcessRequest<T, P>(
            Func<T, P, Player, Task> callback,
            Action<P, Exception, int> exceptionHandler = null,
            Action<P, StoreException, int> storeExceptionHandler = null,
            Action<P, ProgressException, int> progressExceptionHandler = null,
            bool requestOffers = false,
            bool useCache = true)
            where T: HttpRequestBase, new()
            where P: ResponseWithPlayer, new()
        {
            
            stopwatchTotal.Reset();
            stopwatchTotal.Start();
            
            var input = await Request.GetRawBodyBytesAsync();

            var request = SerializerFactory.DeserializeAs<T>(input);//MsOperationRequest.Deserialize(input) as T;  
            var response = new P();
            int playerId = 0;
            try
            {                
                if (request == null)
                    throw new Exception($"Can not cast request");
                               
                //validate request
                ValidateRequest(request);
                
                //init playerBl
                InitPlayerBL();
             
                //validate token
                playerId = await ValidateToken(request.SessionId);

                var player = await PlayerStorage.GetPlayer(playerId, useCache:useCache);
                if (player == null)
                    throw new Exception($"Player {playerId} was not found");

                
                await UpdatePlayerCalculatedFields(player);

                
                //call method logic
                await callback(request, response, player);

                await UpdatePlayerCalculatedFields(player);
                
                //save to cache
                PlayerStorage.PutPlayerToCache();


                response.Player = player;

                
                //there are lots of cases then response is not success and there is not exception
                if (!response.Success)
                    Cacher.RemoveFromCache(playerId);


            }
            catch (StoreException ex)
            {    
                Cacher.RemoveFromCache(playerId);
                storeExceptionHandler?.Invoke(response, ex, playerId);
            }
            catch (ProgressException ex)
            {
                Cacher.RemoveFromCache(playerId);
                progressExceptionHandler?.Invoke(response, ex, playerId);
            }
            catch (Exception ex)
            {
                Cacher.RemoveFromCache(playerId);
                if (exceptionHandler != null)
                    exceptionHandler(response, ex, playerId);
                else
                {
                    response.SetError(ex.Message);                    
                    LogError($"{ControllerContext.ActionDescriptor.ActionName} error (playerId = {playerId}): {ex}");
                }
            }
            
            stopwatchTotal.Stop();
            LogInfo($"{ControllerContext.ActionDescriptor.ActionName} total ms: {stopwatchTotal.ElapsedMilliseconds}");

            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }
        
        protected async Task LogError(string message)
        {
            Logger.Error("BackEnd.Web",
                $"{ControllerContext.ActionDescriptor.ControllerName}.{ControllerContext.ActionDescriptor.ActionName}",
                message);
        }

        protected async Task LogInfo(string message)
        {
            Logger.Info("BackEnd.Web",
                $"{ControllerContext.ActionDescriptor.ControllerName}.{ControllerContext.ActionDescriptor.ActionName}",
                message);
        }
        
        protected async Task LogWarning(string message)
        {
            Logger.Warning("BackEnd.Web",
                $"{ControllerContext.ActionDescriptor.ControllerName}.{ControllerContext.ActionDescriptor.ActionName}",
                message);
        }
        
        protected void SaySlackBot(string text, string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                channelId = devChannelId;
            
            var uri = new Uri("https://slack.com/api/chat.postMessage?token="
                            + Config.Value.SlackBotToken + "&channel=" + channelId + "&text=" + text);

            var httpClient = new HttpClient();
            var result = httpClient.GetAsync(uri).Result;
        }

        protected void ValidateRequest(HttpRequestBase request)
        {
            if (request == null)
                throw new Exception("Request is null");
        }

        protected void InitPlayerBL()
        {
            if (!StorageContainer.IsReadyForRequests())
                throw new Exception("Data Container was not initialized");
            
            playerBL = new PlayerBL(PlayerRepo, StorageContainer.GetStorage(), Cacher, PlayerStorage, Logger);
        }

        protected async Task<int> ValidateToken(Guid token)
        {
            //var playerId = TokenManager.GetPlayerId(token);
            var playerId = await Cacher.GetPlayerId(token);
            if (playerId <= 0)
                throw new Exception($"ValidateToken: Session validation failed (userId = {token})");

            return playerId;
        }
    }
}