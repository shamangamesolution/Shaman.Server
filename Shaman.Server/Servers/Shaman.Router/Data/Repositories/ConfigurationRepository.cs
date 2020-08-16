using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Servers;
using Shaman.DAL.SQL.Repositories;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Data.Repositories
{
    public class ConfigurationRepository : RepositoryBase, IConfigurationRepository
    {
        public ConfigurationRepository(IRouterSqlDalProvider sqlDalProvider) : base(sqlDalProvider.Get())
        {
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
                    PeerCount = GetInt(dt.Rows[i]["peers_count"]),
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
            const string getAllServersSql = @"SELECT `servers`.`id`,
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
                            FROM `servers`";

            return GetServerInfoListFromDataTable(await Dal.Select(getAllServersSql));
        }

        public async Task<EntityDictionary<BundleInfo>> GetBundlesInfo()
        {
            const string bundlesInfoSql = @"SELECT `bundles`.`id`,
                                `bundles`.`server_id`,
                                `bundles`.`uri`
                            FROM `bundles`";

            return GetBundleInfoListFromDataTable(await Dal.Select(bundlesInfoSql));
        }

        public async Task<List<int>> GetServerId(ServerIdentity identity)
        {
            const string sql = @"SELECT `servers`.`id`, `servers`.`address`, `servers`.`ports` 
                            FROM `servers`
                            WHERE `servers`.`address` = ?address and `servers`.`ports` = ?ports";

            return GetIdList(await Dal.Select(sql,
                new MySqlParameter("?address", identity.Address),
                new MySqlParameter("?ports", identity.PortsString)
            ));
        }

        public async Task<int> CreateServerInfo(ServerInfo serverInfo)
        {
            const string sql = @"INSERT INTO `servers`
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
                                (?address,
                                 ?ports,
                                 ?server_role,
                                 ?name,
                                '',
                                ?client_version,
                                ?actualized_on,
                                ?http_port,
                                ?https_port)";

            return (int) await Dal.Insert(sql,
                new MySqlParameter("?address", serverInfo.Address),
                new MySqlParameter("?ports", serverInfo.Ports),
                new MySqlParameter("?server_role", serverInfo.ServerRole),
                new MySqlParameter("?name", serverInfo.Name),
                new MySqlParameter("?client_version", serverInfo.ClientVersion),
                new MySqlParameter("?actualized_on", serverInfo.ActualizedOn),
                new MySqlParameter("?http_port", serverInfo.HttpPort),
                new MySqlParameter("?Https_port", serverInfo.HttpsPort)
            );
        }

        public async Task UpdateServerInfoActualizedOn(int id, int peerCount, ushort httpPort, ushort httpsPort)
        {
            const string sql = @"UPDATE `servers`
                                SET 
                                    `servers`.`actualized_on` = ?date, 
                                    `servers`.`peers_count` = ?peer_count,
                                    `servers`.`http_port` = ?http_port,
                                    `servers`.`https_port` = ?https_port
                                WHERE `servers`.`id` = ?id";
            await Dal.Update(sql,
                new MySqlParameter("?id", id),
                new MySqlParameter("?date", DateTime.UtcNow),
                new MySqlParameter("?peer_count", peerCount),
                new MySqlParameter("?http_port", httpPort),
                new MySqlParameter("?https_port", httpsPort)
            );
        }
    }
}