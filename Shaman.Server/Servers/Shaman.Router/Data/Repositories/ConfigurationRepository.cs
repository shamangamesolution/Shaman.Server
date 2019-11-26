using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Servers;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.MM;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;

namespace Shaman.Router.Data.Repositories
{
    public class ConfigurationRepository : RepositoryBase, IConfigurationRepository
    {

        public ConfigurationRepository(IOptions<RouterConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServer, config.Value.DbName, config.Value.DbUser, config.Value.DbPassword,
                config.Value.DbMaxPoolSize, logger);
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
        
        private EntityDictionary<ServerInfo> GetServerInfoListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<ServerInfo>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new ServerInfo
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Address = GetString(dt.Rows[i]["address"]),
                    Name = GetString(dt.Rows[i]["name"]),
                    Ports = GetString(dt.Rows[i]["ports"]),
                    Region = GetString(dt.Rows[i]["region"]),
                    ActualizedOn = GetNullableDateTime(dt.Rows[i]["actualized_on"]),
                    ClientVersion = GetString(dt.Rows[i]["client_version"]),
                    IsApproved = GetBoolean(dt.Rows[i]["is_approved"]),
                    ServerRole = (ServerRole)GetByte(dt.Rows[i]["server_role"]),
                    HttpPort = GetUshort(dt.Rows[i]["http_port"]),
                    HttpsPort = GetUshort(dt.Rows[i]["https_port"]),
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

        public async Task<EntityDictionary<ServerInfo>> GetAllServerInfo()
        {
            try
            {

                var sql = $@"SELECT `servers`.`id`,
                                `servers`.`address`,
                                `servers`.`ports`,
                                `servers`.`server_role`,
                                `servers`.`name`,
                                `servers`.`region`,
                                `servers`.`client_version`,
                                `servers`.`actualized_on`,
                                `servers`.`is_approved`,
                                `servers`.`peers_count`,
                                `servers`.`http_port`,
                                `servers`.`https_port`
                            FROM `{DbName}`.`servers`";

                return GetServerInfoListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetAllServerInfo)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetAllServerInfo)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<int?> GetServerId(ServerIdentity identity)
        {
            try
            {
                var sql = $@"SELECT `servers`.`id`, `servers`.`address`, `servers`.`ports` 
                            FROM `{DbName}`.`servers`
                            WHERE `servers`.`address` = {Value(identity.Address)} and `servers`.`ports` = {Value(identity.PortsString)}";

                return GetId(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetServerId)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetServerId)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<int> CreateServerInfo(ServerInfo serverInfo)
        {
            try
            {

                var sql = $@"INSERT INTO `{DbName}`.`servers`
                                (`address`,
                                `ports`,
                                `server_role`,
                                `name`,
                                `region`,
                                `client_version`,
                                `actualized_on`,
                                `http_port`,
                                `https_port`)
                                VALUES
                                ({Value(serverInfo.Address)},
                                {Value(serverInfo.Ports)},
                                {Value((byte)serverInfo.ServerRole)},
                                {Value(serverInfo.Name)},
                                {Value(serverInfo.Region)},
                                {Value(serverInfo.ClientVersion)},
                                {Value(serverInfo.ActualizedOn)},
                                {Value(serverInfo.HttpPort)},
                                {Value(serverInfo.HttpsPort)})";

                return (int)(await dal.Insert(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.CreateServerInfo)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.CreateServerInfo)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task UpdateServerInfoActualizedOn(int id, int peerCount, string name, string region, ushort httpPort, ushort httpsPort)
        {
            try
            {

                var sql = $@"UPDATE `{DbName}`.`servers`
                                SET 
                                    `servers`.`actualized_on` = {Value(DateTime.UtcNow)}, 
                                    `servers`.`peers_count` = {Value(peerCount)},
                                    `servers`.`name` = {Value(name)},
                                    `servers`.`region` = {Value(region)},
                                    `servers`.`http_port` = {Value(httpPort)},
                                    `servers`.`https_port` = {Value(httpsPort)}
                                WHERE `servers`.`id` = {Value(id)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.UpdateServerInfoActualizedOn)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.UpdateServerInfoActualizedOn)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
    }
}