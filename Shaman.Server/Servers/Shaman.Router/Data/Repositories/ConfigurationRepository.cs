using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shaman.Contract.Common.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

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
        private EntityDictionary<BundleInfo> GetBundleInfoListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<BundleInfo>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new BundleInfo
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    ServerId = GetInt(dt.Rows[i]["server_id"]),
                    Uri = GetString(dt.Rows[i]["uri"])
                });
            }

            return result;
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
        
        public async Task<EntityDictionary<BundleInfo>> GetBundlesInfo()
        {
            try
            {

                var sql = $@"SELECT `bundles`.`id`,
                                `bundles`.`server_id`,
                                `bundles`.`uri`
                            FROM `{DbName}`.`bundles`";

                return GetBundleInfoListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetBundlesInfo)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ConfigurationRepository)}.{nameof(this.GetBundlesInfo)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<List<int>> GetServerId(ServerIdentity identity)
        {
            try
            {
                var sql = $@"SELECT `servers`.`id`, `servers`.`address`, `servers`.`ports` 
                            FROM `{DbName}`.`servers`
                            WHERE `servers`.`address` = {Value(identity.Address)} and `servers`.`ports` = {Value(identity.PortsString)}";

                return GetIdList(await dal.Select(sql));
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
                                '',
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

        public async Task UpdateServerInfoActualizedOn(int id, int peerCount, ushort httpPort, ushort httpsPort)
        {
            try
            {

                var sql = $@"UPDATE `{DbName}`.`servers`
                                SET 
                                    `servers`.`actualized_on` = {Value(DateTime.UtcNow)}, 
                                    `servers`.`peers_count` = {Value(peerCount)},
                                    `servers`.`http_port` = {Value(httpPort)},
                                    `servers`.`https_port` = {Value(httpsPort)}
                                WHERE `servers`.`id` = {Value(id)}";

                await dal.Update(sql);
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