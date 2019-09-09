using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shaman.BackEnd.Config;
using Shaman.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.General.Entity.Wallet;

namespace Shaman.BackEnd.Data.Repositories
{
    public class StorageRepository : RepositoryBase, IStorageRepository
    {
        
        private const string GlobalParametersTableName = "global_parameters";
        
        public StorageRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServer, config.Value.DbName, config.Value.DbUser, config.Value.DbPassword, logger);
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
                    IsRealCurrency = GetBoolean(dt.Rows[i]["is_real_currency"])
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
                                `currency`.`is_real_currency`
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
        
        public async Task<DataStorage> GetStorage()
        {
            DataStorage storage = new DataStorage();

            storage.Parameters = await GetAllParameters();
            storage.Currencies = await GetCurrencies();
            
            //return storage
            return storage;
        }
    }
}