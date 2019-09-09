using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shaman.BackEnd.Config;
using Shaman.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages.General.Entity;

namespace Shaman.BackEnd.Data.Repositories
{
    public class ParametersRepository : RepositoryBase, IParametersRepository
    {
        public ParametersRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServer, config.Value.DbName, config.Value.DbUser, config.Value.DbPassword, logger);
        }
        
        private List<GlobalParameter> GetParameterListFromDataTable(DataTable dt)
        {
            var result = new List<GlobalParameter>();

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



        private async Task<GlobalParameter> getParameter(string name)
        {            
            var sql = $@"SELECT `global_parameters`.`id`,
                    `global_parameters`.`name`,
                    `global_parameters`.`string_value`,
                    `global_parameters`.`int_value`,
                    `global_parameters`.`float_value`,
                    `global_parameters`.`bool_value`,
                    `global_parameters`.`datetime_value`
                FROM `{DbName}`.`global_parameters`
                WHERE `global_parameters`.`name` = '{name}'";

            var paramList = GetParameterListFromDataTable(await dal.Select(sql));

            if (paramList == null || paramList.Count == 0)
                throw new Exception($"Parameter {name} was not found");

            return paramList[0];
        }

        public async Task<bool> GetBoolValue(string name)
        {
            try
            { 
                return (await getParameter(name)).GetBoolValue();
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetBoolValue)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetBoolValue)}",
                    ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<float> GetFloatValue(string name)
        {
            try
            { 
                return (await getParameter(name)).GetFloatValue();
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetFloatValue)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetFloatValue)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<int> GetIntValue(string name)
        {
            try
            { 
                return (await getParameter(name)).GetIntValue();
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetIntValue)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetIntValue)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<string> GetStringValue(string name)
        {
            try
            { 
                return (await getParameter(name)).GetStringValue();
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetStringValue)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ParametersRepository)}.{nameof(this.GetStringValue)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
    }
}