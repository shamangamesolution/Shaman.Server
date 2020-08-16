using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Shaman.DAL.SQL
{
    public class AutoConnectSqlDal : ISqlDal
    {
        private readonly ISqlDal _sqlDal;

        public AutoConnectSqlDal(SqlDal sqlDal)
        {
            _sqlDal = sqlDal;
        }

        public void Dispose()
        {
        }

        public Task OpenConnection()
        {
            return Task.CompletedTask;
        }

        public Task CloseConnection()
        {
            return Task.CompletedTask;
        }

        public async Task<DataTable> Select(string sqlQuery, params MySqlParameter[] parameters)
        {

            try
            {
                await _sqlDal.OpenConnection();
                return await _sqlDal.Select(sqlQuery, parameters);
            }
            finally
            {
                await _sqlDal.CloseConnection();
            }
        }

        public async Task<long> Insert(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                await _sqlDal.OpenConnection();
                return await _sqlDal.Insert(sqlQuery, parameters);
            }
            finally
            {
                await _sqlDal.CloseConnection();
            }

        }

        public async Task Update(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                await _sqlDal.OpenConnection();
                await _sqlDal.Update(sqlQuery, parameters);
            }
            finally
            {
                await _sqlDal.CloseConnection();
            }

        }

        public async Task Delete(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                await _sqlDal.OpenConnection();
                await _sqlDal.Delete(sqlQuery, parameters);
            }
            finally
            {
                await _sqlDal.CloseConnection();
            }

        }

        public async Task<int> Execute(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                await _sqlDal.OpenConnection();
                return await _sqlDal.Execute(sqlQuery, parameters);
            }
            finally
            {
                await _sqlDal.CloseConnection();
            }
        }
    }
}