using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.Shared.Data.Entity.Timers;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;

namespace Sample.BackEnd.Data.Timers
{
    public class TimerRepository : RepositoryBase, ITimerRepository
    {
        public TimerRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServerTemp, config.Value.DbNameTemp, config.Value.DbUserTemp, config.Value.DbPasswordTemp, config.Value.DbMaxPoolSize,
                logger);
            
        }
        
        private const string TimersTableName = "timers";
        
        private List<Timer> GetTimersListFromDataTable(DataTable dt)
        {
            var result = new List<Timer>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                
                result.Add(new Timer
                {
                    Id = GetInt(dt.Rows[i]["id"]),
                    Type = (TimerType) GetByte(dt.Rows[i]["type"]),
                    PlayerId = GetInt(dt.Rows[i]["player_id"]),
                    StartedOn = GetDateTime(dt.Rows[i]["started_on"]),
                    RelatedObjectId = GetInt(dt.Rows[i]["related_object_id"]),
                    SecondsToComplete = GetInt(dt.Rows[i]["seconds_to_complete"])
                });
            }

            return result;
        }
        
        public async Task StartTimer(int playerId, TimerType timerType, int relatedObjectId, int secondsToComplete)
        {
            try
            {

                var sql = $@"INSERT INTO `{DbName}`.`{TimersTableName}`
                            (`type`,
                            `started_on`,
                            `seconds_to_complete`,
                            `player_id`,
                            `related_object_id`)
                            VALUES
                            ({Value((byte)timerType)},
                            {Value(DateTime.UtcNow)},
                            {Value(secondsToComplete)},
                            {Value(playerId)},
                            {Value(relatedObjectId)})";

                await dal.Insert(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(TimerRepository)}.{nameof(this.StartTimer)}", ex.ToString());                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(TimerRepository)}.{nameof(this.StartTimer)}", ex.ToString());                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task<List<Timer>> GetTimer(int playerId)
        {
            try
            {

                var sql = $@"SELECT `{TimersTableName}`.`id`,
                                `{TimersTableName}`.`type`,
                                `{TimersTableName}`.`started_on`,
                                `{TimersTableName}`.`seconds_to_complete`,
                                `{TimersTableName}`.`player_id`,
                                `{TimersTableName}`.`related_object_id`
                            WHERE `{TimersTableName}`.`player_id` = {Value(playerId)}
                            FROM `{DbName}`.`{TimersTableName}`";

                return GetTimersListFromDataTable(await dal.Select(sql));
            }
            catch (DalException ex)
            {
                LogError($"{typeof(TimerRepository)}.{nameof(this.GetTimer)}", ex.ToString());                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(TimerRepository)}.{nameof(this.GetTimer)}", ex.ToString());                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

        public async Task StopTimer(int timerId)
        {
            try
            {

                var sql = $@"DELETE FROM `{DbName}`.`{TimersTableName}`
                            WHERE `{TimersTableName}`.`id` = {Value(timerId)}";

                await dal.Delete(sql);
            }
            catch (DalException ex)
            {
                LogError($"{typeof(TimerRepository)}.{nameof(this.StopTimer)}", ex.ToString());                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(TimerRepository)}.{nameof(this.StopTimer)}", ex.ToString());                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
    }
}