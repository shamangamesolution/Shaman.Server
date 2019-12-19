using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Authentication;
using Sample.Shared.Data.Entity.Currency;
using Sample.Shared.Data.Entity.Progress;
using Sample.Shared.Data.Entity.Shopping;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages;
using Shaman.Messages.General.Entity;

namespace Sample.BackEnd.Data.Repositories
{
    public class StorageRepository : RepositoryBase, IStorageRepository
    {
        
        private const string GlobalParametersTableName = "global_parameters";
        private const string AuthProvidersTableName = "auth_providers";
        private const string PlayerLevelsTableName = "player_levels";
        
        private readonly IShamanLogger _logger;
        public StorageRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            _logger = logger;
            Initialize(config.Value.DbServerStatic, config.Value.DbNameStatic, config.Value.DbUserStatic, config.Value.DbPasswordStatic, config.Value.DbMaxPoolSize,
                logger);
        }
        
        #region parameters
        
        #region mappers
        private EntityDictionary<GlobalParameter> GetParameterListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<GlobalParameter>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new GlobalParameter
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Name = GetString(dt.Rows[i]["name"]),
                    BoolValue = GetBoolean(dt.Rows[i]["bool_value"]),
                    FloatValue = GetFloat(dt.Rows[i]["float_value"]),
                    IntValue = GetInt(dt.Rows[i]["int_value"]),
                    StringValue = GetString(dt.Rows[i]["string_value"]),
                    DateTimeValue = GetNullableDateTime(dt.Rows[i]["datetime_value"])
                });
            }

            return result;
        }     
        #endregion

        private async Task<EntityDictionary<GlobalParameter>> GetAllParameters()
        {
            try
            { 
                var sql = $@"SELECT `{GlobalParametersTableName}`.`id`,
                        `{GlobalParametersTableName}`.`name`,
                        `{GlobalParametersTableName}`.`string_value`,
                        `{GlobalParametersTableName}`.`int_value`,
                        `{GlobalParametersTableName}`.`float_value`,
                        `{GlobalParametersTableName}`.`bool_value`,
                        `{GlobalParametersTableName}`.`datetime_value`
                    FROM `{DbName}`.`{GlobalParametersTableName}`";

                return GetParameterListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetAllParameters)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetAllParameters)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        #endregion
        
        #region currencies
        #region mapping Helper
        private EntityDictionary<Currency> GetPCurrenciesListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<Currency>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new Currency
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Type = (CurrencyType) GetByte(dt.Rows[i]["type"]),
                    IsRealCurrency = GetBoolean(dt.Rows[i]["is_real_currency"]),
                    RelatedObjectId = GetInt(dt.Rows[i]["related_object_id"]),
                    RelatedObjectType = (GameItemType)GetByte(dt.Rows[i]["related_object_type"])
                });
            }

            return result;
        }
        #endregion

        private async Task<EntityDictionary<Currency>> GetCurrencies()
        {
            try
            {

                var sql = $@"SELECT `currency`.`id`,
                                `currency`.`type`,
                                `currency`.`is_real_currency`,
                                `currency`.`related_object_type`,
                                `currency`.`related_object_id`
                            FROM `{DbName}`.`currency`";

                return GetPCurrenciesListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetCurrencies)}", ex.ToString());                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetCurrencies)}", ex.ToString());                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        #endregion
        
        #region auth providers
        #region mapping Helper
        private EntityDictionary<AuthProvider> GetAuthProviderListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<AuthProvider>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new AuthProvider
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Name = GetString(dt.Rows[i]["name"])
                });
            }

            return result;
        }
        #endregion

        private async Task<EntityDictionary<AuthProvider>> GetAuthProviders()
        {
            try
            {

                var sql = $@"SELECT `{AuthProvidersTableName}`.`id`,
                                `{AuthProvidersTableName}`.`name`
                            FROM `{DbName}`.`{AuthProvidersTableName}`";

                return GetAuthProviderListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetAuthProviders)}", ex.ToString());                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetAuthProviders)}", ex.ToString());                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        #endregion
        
        #region progress
        private EntityDictionary<PlayerLevel> GetPlayerLevelListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<PlayerLevel>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new PlayerLevel()
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Level = GetInt(dt.Rows[i]["level"]),
                    Experience = GetInt(dt.Rows[i]["experience"]),
                });
            }

            return result;
        }
       
        public async Task<EntityDictionary<PlayerLevel>> GetPlayerLevels()
        {
            try
            {
                var sql = $@"SELECT `{PlayerLevelsTableName}`.`id`,
                                `{PlayerLevelsTableName}`.`level`,
                                `{PlayerLevelsTableName}`.`experience`
                            FROM `{DbName}`.`{PlayerLevelsTableName}`";

                return GetPlayerLevelListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetPlayerLevels)}", ex.ToString());                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetPlayerLevels)}", ex.ToString());                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        #endregion
        
        #region shop items
        private EntityDictionary<ShopItem> GetShopItemListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<ShopItem>();

            if (dt == null || dt.Rows.Count == 0)
                
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new ShopItem
                {
                    ConditionValue = GetInt(dt.Rows[i]["condition_value"]),
                    CurrencyId = GetInt(dt.Rows[i]["currency"]),
                    Id = GetInt(dt.Rows[i]["id"]),
                    Price = GetFloat(dt.Rows[i]["price"]),
                    ItemId = GetInt(dt.Rows[i]["item_id"]),
                    SortOrder = GetInt(dt.Rows[i]["sort_order"]),
                    ConditionType = (ConditionType)GetInt(dt.Rows[i]["condition_type"]),
                    ExternalId = GetString(dt.Rows[i]["external_id"]),
                    ItemQuantity = GetInt(dt.Rows[i]["item_quantity"]),
                    IsSpecialOffer = GetBoolean(dt.Rows[i]["is_special_offer"]),
                    Enabled = GetBoolean(dt.Rows[i]["enabled"]),
                    OldPrice = GetFloat(dt.Rows[i]["old_price"]),
                    OldExternalId = GetString(dt.Rows[i]["old_external_id"]),
                    Discount = GetInt(dt.Rows[i]["discount"]),
                    ItemType = (GameItemType) GetByte(dt.Rows[i]["item_type"])
                });
            }

            return result;
        }
        
        public async Task<EntityDictionary<ShopItem>> GetShopItems()
        {
            try
            {
                var sql = $@"SELECT `shop_items`.`id`,
                                `shop_items`.`external_id`,
                                `shop_items`.`item_type`,
                                `shop_items`.`item_id`,
                                `shop_items`.`item_quantity`,
                                `shop_items`.`condition_type`,
                                `shop_items`.`condition_value`,
                                `shop_items`.`currency`,
                                `shop_items`.`price`,
                                `shop_items`.`sort_order`,
                                `shop_items`.`is_special_offer`,
                                `shop_items`.`enabled`,
                                `shop_items`.`old_price`,
                                `shop_items`.`old_external_id`,
                                `shop_items`.`discount`
                        FROM `{DbName}`.`shop_items`";

                return GetShopItemListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetShopItems)}", ex.ToString());                                                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(StorageRepository)}.{nameof(this.GetShopItems)}", ex.ToString());                                                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        #endregion
        
        public async Task<DataStorage> GetStorage()
        {
            DataStorage storage = new DataStorage(_logger);
            
            storage.Parameters = await GetAllParameters();
            storage.Currencies = await GetCurrencies();
            storage.AuthProviders = await GetAuthProviders();
            storage.PlayerLevels = await GetPlayerLevels();
            storage.ShopItems = await GetShopItems();

            storage.FillRelatedEntities();

            storage.ConsistencyCheck();
            
            //return storage
            return storage;
        }
    }
}