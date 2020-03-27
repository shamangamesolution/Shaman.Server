using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Currency;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages;

namespace Sample.BackEnd.Data.Repositories
{
    public class PlayerRepository : RepositoryBase, IPlayerRepository
    {
        private IStorageContainer _storageContainer;    
        
        public PlayerRepository(IOptions<BackendConfiguration> config, IShamanLogger logger, IStorageContainer storageContainer)
        {
            _storageContainer = storageContainer;
            Initialize(config.Value.DbServer, config.Value.DbName, config.Value.DbUser, config.Value.DbPassword,config.Value.DbMaxPoolSize,
                logger);
        }

        #region mapping Helper

        private EntityDictionary<PlayerWalletItem> GetWalletItemListFromDataTable(DataTable dt)
        {
            var result = new EntityDictionary<PlayerWalletItem>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new PlayerWalletItem
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    PlayerId = GetInt(dt.Rows[i]["player_id"]),
                    CurrencyId = GetInt(dt.Rows[i]["currency_id"]),
                    Quantity = GetUInt(dt.Rows[i]["quantity"])   
                });
            }

            return result;
        }

        private List<Player> GetPlayerListFromDataTable(DataTable dt)
        {
            var result = new List<Player>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for(int i=0;i<dt.Rows.Count;i++)
            {
                result.Add(new Player
                {
                    Blocked = GetBoolean(dt.Rows[i]["blocked"]),
                    Experience = GetInt(dt.Rows[i]["experience"]),
                    Id = GetInt(dt.Rows[i]["id"]),
                    LastOnline = GetDateTime(dt.Rows[i]["last_online"]),
                    Level = GetByte(dt.Rows[i]["level"]),
                    NickName = GetString(dt.Rows[i]["nickname"]),
                    GuestId = GetString(dt.Rows[i]["guest_id"]),
                    RegistrationDate = GetDateTime(dt.Rows[i]["registration_date"]),
                });
            }

            return result;       
        }
        
        private int GetPlayerIdFromDataTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return 0;

            return GetInt(dt.Rows[0]["id"]);

        }
        #endregion
        
        public async Task ChangeName(int playerId, string newName)
        {

            try
            {
                var sql = $@"UPDATE {DbName}.`players`
                                SET
                                `nickname` = {Value(ClearStringData(newName))}
                                WHERE `id` = {Value(playerId)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.ChangeName)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.ChangeName)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        private async Task<int> CreatePlayer(Player player)
        {
            try
            {
                var sql = $@"INSERT INTO {DbName}.`players`
                            (`guest_id`,
                            `nickname`,
                            `registration_date`,
                            `last_online`,
                            `blocked`,
                            `level`,
                            `experience`)
                            VALUES
                            ({Value(player.GuestId)},
                            {Value(player.NickName)},
                            {Value(player.RegistrationDate)},
                            {Value(player.LastOnline)},
                            {Value(GetMySQLTinyInt(player.Blocked))},
                            {Value(player.Level)},
                            {Value(player.Experience)})";

                var playerId = (int)(await dal.Insert(sql));

                return playerId;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreatePlayer)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreatePlayer)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        
        public async Task<int> GetPlayerId(string guestId)
        {
            try
            {
                var sql = $@"SELECT `players`.`id`,
                            `players`.`guest_id`
                        FROM {DbName}.`players`
                        WHERE `players`.`guest_id` = {Value(guestId)}";

                var data = await dal.Select(sql);

                //convert to list
                var playerId = GetPlayerIdFromDataTable(data);

                //check
                if(playerId == 0)
                    throw new DalException(DalExceptionCode.PlayerNotFound, $"Player with id {guestId} not found");

                return playerId;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetPlayerId)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetPlayerId)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<Player> GetPlayerInfo(int playerId)
        {
            try
            {
                var sql = $@"SELECT `players`.`id`,
                            `players`.`guest_id`,
                            `players`.`nickname`,
                            `players`.`registration_date`,
                            `players`.`last_online`,
                            `players`.`blocked`,
                            `players`.`level`,
                            `players`.`experience`
                        FROM {DbName}.`players`
                        WHERE `players`.`id` = {Value(playerId)}";

                var data = await dal.Select(sql);

                //convert to list
                var playerList = GetPlayerListFromDataTable(data);

                //check
                if (playerList.Any())
                {
                    var player = playerList[0];

                    player.Wallet = new PlayerWallet
                    {
                        Items = new EntityDictionary<PlayerWalletItem>((await GetWalletItems(player.Id)))
                    };
                   
                    return player;
                }
                else
                    throw new DalException(DalExceptionCode.PlayerNotFound, $"Player with id {playerId} not found");
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetPlayerInfo)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetPlayerInfo)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        public async Task UpdateLastOnlineDate(string guestId)
        {
            try
            {
                var sql = $@"UPDATE {DbName}.`players`
                                SET
                                `last_online` = {Value(DateTime.UtcNow)}
                                WHERE `players`.`guest_id` = {Value(guestId)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.UpdateLastOnlineDate)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.UpdateLastOnlineDate)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        public async Task UpdateLastOnlineDate(int playerId)
        {
            try
            {
                var sql = $@"UPDATE {DbName}.`players`
                                SET
                                `last_online` = {Value(DateTime.UtcNow)}
                                WHERE `players`.`id` = {Value(playerId)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.UpdateLastOnlineDate)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.UpdateLastOnlineDate)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        public async Task UpdateLevel(int playerId, byte newLevel)
        {
            try
            {
                var sql = $@"UPDATE {DbName}.`players`
                                SET
                                `level` = {Value(newLevel)}
                                WHERE `id` = {Value(playerId)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.UpdateLevel)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.UpdateLevel)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<int> GetPlayerIdByGuestId(string guestId)
        {
            try
            {
                var sql = $@"SELECT `players`.`id`,
                            `players`.`guest_id`
                        FROM {DbName}.`players`
                        WHERE `players`.`guest_id` = {Value(ClearStringData(guestId))}";

                return GetPlayerIdFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetPlayerIdByGuestId)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetPlayerIdByGuestId)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<Player> CreatePlayer(string guestId)
        {
            try
            {
                //default player
                var player = Player.GetDefaultPlayer();
                //get default league from parameters
                
                player.GuestId = guestId;

                //create default player
                var id = await CreatePlayer((Player) player);
                
                //assign id
                player.Id = id;

                return player;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreatePlayer)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreatePlayer)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<int> CreateWalletItem(PlayerWalletItem item)
        {
            try
            {
                string sql = $@"INSERT INTO `{DbName}`.`player_wallet_item`
                                (`player_id`,
                                `currency_id`,
                                `quantity`)
                                VALUES
                                ({Value(item.PlayerId)},
                                {Value(item.CurrencyId)},
                                {Value(item.Quantity)});";

                var id = (int)(await dal.Insert(sql));

                return id;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreateWalletItem)}", ex.ToString());                                                              
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreateWalletItem)}", ex.ToString());                                                              
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        
        public async Task<EntityDictionary<PlayerWalletItem>> GetWalletItems(int playerId)
        {
            try
            {
                var sql = $@"SELECT `player_wallet_item`.`id`,
                            `player_wallet_item`.`player_id`,
                            `player_wallet_item`.`currency_id`,
                            `player_wallet_item`.`quantity`
                        FROM `{DbName}`.`player_wallet_item`
                        WHERE `player_wallet_item`.`player_id` = {Value(playerId)}";

                var data = await dal.Select(sql);
                var items = GetWalletItemListFromDataTable(data);

                //convert to list
                return items;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetWalletItems)}", ex.ToString());                                                              
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.GetWalletItems)}", ex.ToString());                                                              
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        
        public async Task UpdateWalletItem(int playerWalletItemId, uint quantity)
        {
            try
            {
                string sql = $@"UPDATE `{DbName}`.`player_wallet_item`
                                SET `player_wallet_item`.`quantity` = {Value(quantity)}
                                WHERE `player_wallet_item`.`id` = {Value(playerWalletItemId)}";

                dal.Update(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreateWalletItem)}", ex.ToString());                                                              
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(PlayerRepository)}.{nameof(this.CreateWalletItem)}", ex.ToString());                                                              
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

    }
}
