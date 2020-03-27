using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Entity.ExternalAccounts;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages;

namespace Sample.BackEnd.Data.Repositories
{
    public class ExternalAccountsRepository : RepositoryBase, IExternalAccountsRepository
    {
        public ExternalAccountsRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServer, config.Value.DbName, config.Value.DbUser, config.Value.DbPassword,config.Value.DbMaxPoolSize,
                logger);
        }

        private const string ExternalAccountTableName = "external_accounts";
        
        private EntityDictionary<ExternalAccount> GetExternalAccountsListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<ExternalAccount>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var acc = new ExternalAccount(GetInt(dt.Rows[i]["provider_id"]),
                    GetInt(dt.Rows[i]["player_id"]),
                    GetString(dt.Rows[i]["external_id"]),
                    GetString(dt.Rows[i]["guest_id"]))
                {
                    Id = GetInt(dt.Rows[i]["id"])
                };

                result.Add(acc);
            }

            return result;
        }

        public async Task<EntityDictionary<ExternalAccount>> GetExternalAccounts(int providerId, string externalId)
        {
            try
            {
                string sql = $@"SELECT `{ExternalAccountTableName}`.`id`,
                                    `{ExternalAccountTableName}`.`provider_id`,
                                    `{ExternalAccountTableName}`.`player_id`,
                                    `{ExternalAccountTableName}`.`external_id`,
                                    `{ExternalAccountTableName}`.`guest_id`
                                FROM `{DbName}`.`{ExternalAccountTableName}`
                                WHERE `{ExternalAccountTableName}`.`provider_id` = {Value(providerId)}
                                        and `{ExternalAccountTableName}`.`external_id` = {Value(ClearStringData(externalId))}";

                return GetExternalAccountsListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.GetExternalAccounts)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.GetExternalAccounts)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<string> GetGuestId(int id)
        {
            try
            {
                string sql = $@"SELECT `{ExternalAccountTableName}`.`id`,
                                    `{ExternalAccountTableName}`.`provider_id`,
                                    `{ExternalAccountTableName}`.`player_id`,
                                    `{ExternalAccountTableName}`.`external_id`,
                                    `{ExternalAccountTableName}`.`guest_id`
                                FROM `{DbName}`.`{ExternalAccountTableName}`
                                WHERE `{ExternalAccountTableName}`.`id` = {Value(id)}";

                var acc = GetExternalAccountsListFromDataTable(await dal.Select(sql)).FirstOrDefault();

                if (acc == null)
                    throw new DalException(DalExceptionCode.ExternalAccountWasNotFound, $"External account {id} was not found");

                return acc.GuestId;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.GetGuestId)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.GetGuestId)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task CreateExternalAccount(ExternalAccount externalAccount)
        {
            try
            {
                string sql = $@"INSERT INTO `{DbName}`.`{ExternalAccountTableName}`
                                (`provider_id`,
                                `player_id`,
                                `external_id`,
                                `guest_id`)
                                VALUES
                                ({Value(externalAccount.AuthProviderId)},
                                {Value(externalAccount.PlayerId)},
                                {Value(externalAccount.ExternalId)},
                                {Value(externalAccount.GuestId)})";

                dal.Insert(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.CreateExternalAccount)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.CreateExternalAccount)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task UnlinkAccount(int authProviderId, int playerId)
        {
            try
            {
                string sql = $@"DELETE FROM `{DbName}`.`{ExternalAccountTableName}`
                                WHERE `{ExternalAccountTableName}`.`provider_id` = {Value(authProviderId)}
                                        and `{ExternalAccountTableName}`.`player_id` = {Value(playerId)}";

                dal.Delete(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.UnlinkAccount)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(ExternalAccountsRepository)}.{nameof(this.UnlinkAccount)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }


    }
}