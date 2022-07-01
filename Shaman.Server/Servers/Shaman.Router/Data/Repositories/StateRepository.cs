using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Shaman.Bundling.Common;
using Shaman.DAL.SQL.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Models;
using Shaman.Serialization.Messages;

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
                CreatedOn = GetDateTime(dt.Rows[i]["created_on"])
            });
        }

        return result;
    }
    
    public async Task<List<StateInfo>> GetStates()
    {
        const string bundlesInfoSql = @"SELECT `states`.`server_id`,
                                `states`.`state`,
                                `states`.`created_on`
                            FROM `states`";

        return GetStateInfoListFromDataTable(await Dal.Select(bundlesInfoSql));
    }

    public async Task SaveState(int serverId, string state, DateTime createdOn)
    {
        const string sql = @"INSERT INTO `states`
                                (`server_id`,
                                `state`,
                                `created_on`)
                                VALUES
                                (?server_id,
                                ?state,
                                ?created_on)";

        await Dal.Insert(sql,
            new MySqlParameter("?server_id", serverId),
            new MySqlParameter("?state", state),
            new MySqlParameter("?created_on", createdOn)
        );
    }
}