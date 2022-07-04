using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Router.Models;

namespace Shaman.Router.Data.Repositories.Interfaces;

public interface IStateRepository
{
    Task<List<StateInfo>> GetStates();
    Task InsertState(int serverId, string state, DateTime createdOn);
    Task UpdateState(int serverId, string state, DateTime actualizedOn);
}