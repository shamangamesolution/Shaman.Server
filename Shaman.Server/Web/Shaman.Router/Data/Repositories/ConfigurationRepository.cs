using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Router;
using Shaman.Router.Data.Repositories.Interfaces;

namespace Shaman.Router.Data.Repositories
{
    public class ConfigurationRepository : RepositoryBase, IConfigurationRepository
    {
        public ConfigurationRepository(string dbServer, string dbName, string dbUser, string dbPassword, IShamanLogger logger) 
            : base(dbServer, dbName, dbUser, dbPassword, logger)
        {
        }

        private int GetSum(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return 0;
            
            return GetInt(dt.Rows[0]["sum"]);
        }
        
        private List<MatchMakerConfiguration> GetMmConfigFromDataTable(DataTable dt)
        {
            var result = new List<MatchMakerConfiguration>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new MatchMakerConfiguration
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Version = GetString(dt.Rows[i]["version"]),
                    Name = GetString(dt.Rows[i]["name"]),
                    Address = GetString(dt.Rows[i]["address"]),
                    Port = GetUshort(dt.Rows[i]["port"]),
                    BackendId = GetInt(dt.Rows[i]["backend_id"]),
                });
            }

            return result;
        }
        
        private List<Backend> GetBackendListFromDataTable(DataTable dt)
        {
            var result = new List<Backend>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new Backend
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Address = GetString(dt.Rows[i]["address"]),
                    Port = GetUshort(dt.Rows[i]["port"])
                });
            }

            return result;
        }
        
        public async Task<int> GetOnline()
        {
            try
            {
                var sql = $@"select (sum(stat.mp) + sum(stat.gp)) as sum from
                            (select distinct region_name, master_peers as mp, game_peers as gp
                            from `{DbName}`.configurations) stat";

                return GetSum(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetOnline)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetOnline)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<List<MatchMakerConfiguration>> GetMmConfigurations(GameProject game, string version)
        {
            var configs = await GetAllMmConfigurations(game);
            return configs.Where(c => c.Version == version).ToList();
        }

        public async Task<List<MatchMakerConfiguration>> GetAllMmConfigurations(GameProject game, bool actualOnly = true)
        {
            try
            {
//                LogInfo($"{typeof(ConfigurationRepository)}.{nameof(this.GetAllConfigurations)}",
//                    $"Configurations requested for {game}");
                var sql = $@"SELECT `matchmakers`.`id`,
                                `matchmakers`.`game`,
                                `matchmakers`.`version`,
                                `matchmakers`.`name`,
                                `matchmakers`.`address`,
                                `matchmakers`.`port`,
                                `matchmakers`.`approved`,
                                `matchmakers`.`actualized_on`,
                                `matchmakers`.`backend_id`    
                            FROM `{DbName}`.`matchmakers`";
                if (actualOnly)
                    sql += $@" WHERE `matchmakers`.`game` = {Value((byte)game)} and `approved` = 1 and `actualized_on` > {Value(DateTime.UtcNow.Subtract(new TimeSpan(0,10,0)))};";
                else
                {
                    sql += $@" WHERE `matchmakers`.`game` = {Value((byte)game)};";
                }

                return GetMmConfigFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetAllMmConfigurations)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetAllMmConfigurations)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task CreateMmConfiguration(GameProject game, string name, string address, ushort port)
        {
            try
            {
                var sql = $@"INSERT INTO `{DbName}`.`matchmakers`
                                (`game`,
                                `name`,
                                `address`,
                                `port`,
                                `approved`,
                                `actualized_on`,
                                `backend_id`)
                                VALUES
                                ({Value((byte)game)},
                                {Value(name)},
                                {Value(address)},
                                {Value(port)},
                                '1',
                                {Value(DateTime.UtcNow)},
                                '1')";

                dal.Insert(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.CreateMmConfiguration)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.CreateMmConfiguration)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task UpdateMmConfiguration(GameProject game, string name, string address, ushort port)
        {
            try
            {
                var sql = $@"UPDATE `{DbName}`.`matchmakers`
                                SET                                
                                `name` = {Value(name)},
                                `address` = {Value(address)},
                                `port` = {Value(port)},
                                `actualized_on` = {Value(DateTime.UtcNow)}
                                WHERE `game` = {Value((byte)game)} and `address` = {Value(address)} and `port` = {Value(port)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.UpdateMmConfiguration)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.UpdateMmConfiguration)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<List<Backend>> GetBackends()
        {
            try
            {

                var sql = $@"SELECT `backends`.`id`,
                                `backends`.`address`,
                                `backends`.`port`
                            FROM `{DbName}`.`backends`";

                return GetBackendListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetBackends)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetBackends)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
    }
}