using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.BackEnd.Extensions;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;

namespace Sample.BackEnd.Controllers
{
    public class StorageController : WebControllerBase
    {

        public StorageController(IPlayerRepository playerRepo, 
            ITempRepository tempRepo, 
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config,     
            ICacher cacher, 
            IPlayerStorage playerStorage,
            ISerializer serializerFactory,
            IStorageContainer storageContainer) 
            : base(playerRepo, tempRepo, logger, config, cacher, playerStorage, serializerFactory, storageContainer)
        {
        }
        
        [HttpPost]
        public async Task<ActionResult> GetStorage()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = SerializerFactory.DeserializeAs<GetStorageHttpRequest>(input);

            var response = new GetStorageHttpResponse();
            try
            {
                response.SerializedAndCompressedStorage = CompressHelper.Compress(SerializerFactory.Serialize(StorageContainer.GetStorage()));
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
            }
            
            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }
        
        [HttpPost]
        public async Task<ActionResult> GetStorageVersion()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = SerializerFactory.DeserializeAs<GetCurrentStorageVersionRequest>(input);

            var response = new GetCurrentStorageVersionResponse();
            try
            {
                response.CurrentDatabaseVersion = StorageContainer.GetStorage().DatabaseVersion;
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
            }
            
            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }
    }
}