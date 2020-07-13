using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Shaman.DAL.SQL
{
    public interface ISqlDal : IDisposable
    {
        Task OpenConnection();
        Task CloseConnection();
        Task<DataTable> Select(string sqlQuery, params MySqlParameter[] parameters);
        Task<long> Insert(string sqlQuery, params MySqlParameter[] parameters);
        Task Update(string sqlQuery, params MySqlParameter[] parameters);
        Task Delete(string sqlQuery, params MySqlParameter[] parameters);
        Task<int> Execute(string sqlQuery, params MySqlParameter[] parameters);
    }
}