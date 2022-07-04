using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Shaman.DAL.SQL.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Models;

namespace Shaman.Router.Data.Repositories;

public class StateRepository : RepositoryBase, IStateRepository
{
    public StateRepository(IRouterSqlDalProvider sqlDalProvider) : base(sqlDalProvider.Get())
    {
        
    }
    
    private List<StateInfo> GetStateInfoListFromDataTable(DataTable dt)
    {
        var result = new List<StateInfo>();

        if (dt == null || dt.Rows.Count == 0)
            return result;

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            result.Add(new StateInfo
            {
                ServerId = GetInt(dt.Rows[i]["server_id"]),
                SerializedState = GetString(dt.Rows[i]["state"]),
                ActualizeOn = GetDateTime(dt.Rows[i]["actualized_on"]),
                CreatedOn = GetDateTime(dt.Rows[i]["created_on"])
            });
        }

        return result;
    }
    
    public async Task<List<StateInfo>> GetStates()
    {
        const string bundlesInfoSql = @"SELECT `states`.`server_id`,
                                `states`.`state`,
                                `states`.`actualized_on`,
                                `states`.`created_on`
                            FROM `states`";

        return GetStateInfoListFromDataTable(await Dal.Select(bundlesInfoSql));
    }

    public async Task InsertState(int serverId, string state, DateTime createdOn)
    {
        const string sql = @"INSERT INTO `states`
                                (`server_id`,
                                `state`,
                                `created_on`,
                                `actualized_on`)
                                VALUES
                                (?server_id,
                                ?state,
                                ?created_on,
                                ?created_on)";

        await Dal.Insert(sql,
            new MySqlParameter("?server_id", serverId),
            new MySqlParameter("?state", state),
            new MySqlParameter("?created_on", createdOn)
        );
    }

    public async Task UpdateState(int serverId, string state, DateTime actualizedOn)
    {
        const string sql = @"UPDATE `states`
                        SET 
                                `states`.`state` = ?state,
                                `states`.`actualized_on` = ?date
                        WHERE server_id = ?server_id";
        await Dal.Update(sql,
            new MySqlParameter("?server_id", serverId),
            new MySqlParameter("?state", state),
            new MySqlParameter("?date", actualizedOn)
        );
    }
}