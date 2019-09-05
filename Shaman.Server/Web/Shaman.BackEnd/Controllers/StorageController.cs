using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shaman.BackEnd.Config;
using Shaman.BackEnd.Data.PlayerStorage;
using Shaman.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests.Storage;
using Shaman.Messages.General.DTO.Responses.Storage;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Shared.Caching;
using Shaman.Shared.Extensions;

namespace Shaman.BackEnd.Controllers
{
    public class StorageController : WebControllerBase
    {

        public StorageController(IPlayerRepository playerRepo, 
            ITempRepository tempRepo, 
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config,     
            ICacher cacher, 
            IPlayerStorage playerStorage,
            ISerializerFactory serializerFactory,
            IStorageContainer storageContainer) 
            : base(playerRepo, tempRepo, logger, config, cacher, playerStorage, serializerFactory, storageContainer)
        {
        }
        
        [HttpPost]
        public async Task<ActionResult> GetStorage()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = MessageBase.DeserializeAs<GetStorageHttpRequest>(SerializerFactory, input);

            var response = new GetStorageHttpResponse();
            try
            {
                response.SerializedAndCompressedStorage = CompressHelper.Compress(StorageContainer.GetStorage().Serialize(SerializerFactory));
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
            }
            
            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
        
        [HttpPost]
        public async Task<ActionResult> GetNotCompressedStorage()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = MessageBase.DeserializeAs<GetNotCompressedStorageRequest>(SerializerFactory, input);

            var response = new GetNotCompressedStorageResponse();
            try
            {
                response.SerializedStorage = StorageContainer.GetStorage().Serialize(SerializerFactory);
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
            }
            
            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
        
        [HttpPost]
        public async Task<ActionResult> GetStorageVersion()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = MessageBase.DeserializeAs<GetCurrentStorageVersionRequest>(SerializerFactory, input);

            var response = new GetCurrentStorageVersionResponse();
            try
            {
                response.CurrentDatabaseVersion = StorageContainer.GetStorage().DatabaseVersion;
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
            }
            
            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
    }
}