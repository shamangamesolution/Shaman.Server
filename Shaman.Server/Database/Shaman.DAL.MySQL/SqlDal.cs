using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Shaman.DAL.SQL.Exceptions;

namespace Shaman.DAL.SQL
{
    public class SqlDbConfig
    {
        public string Host { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int MaxPoolSize { get; set; }
    }


    public class SqlDal : ISqlDal
    {
        private readonly MySqlConnection _connection;

        //Constructor
        public SqlDal(SqlDbConfig dbConfig)
        {
            _connection = new MySqlConnection(
                $"SERVER={dbConfig.Host};" +
                $"DATABASE={dbConfig.Database};" +
                $"UID={dbConfig.User};" +
                $"PASSWORD={dbConfig.Password};" +
                $"Max Pool Size={dbConfig.MaxPoolSize};");
        }

        //open connection to database
        public async Task OpenConnection()
        {
            try
            {
                await _connection.OpenAsync();
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        throw new SqlDalException("Cannot connect to server.  Contact administrator", ex);
                    case 1045:
                        throw new SqlDalException("Invalid username/password, please try again", ex);
                    default:
                        throw new SqlDalException("General SQL error", ex);
                }
            }
        }

        public async Task CloseConnection()
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch (MySqlException ex)
            {
                throw new SqlDalException("Connection close error", ex);
            }
        }

        public async Task<DataTable> Select(string sqlQuery, params MySqlParameter[] parameters)
        {
            using (var da = new MySqlDataAdapter())
            {
                using (var dt = new DataTable())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(sqlQuery, _connection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddRange(parameters);
                            da.SelectCommand = cmd;
                            await da.FillAsync(dt);
                            return dt;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        throw new SqlDalException($"Error executing SQL", ex);
                    }
                }
            }
        }

        public async Task<long> Insert(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand(sqlQuery, _connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                    return cmd.LastInsertedId;
                }
            }
            catch (MySqlException ex)
            {
                throw new SqlDalException($"Error executing SQL", ex);
            }
        }

        public async Task Update(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand(sqlQuery, _connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new SqlDalException($"Error executing SQL", ex);
            }
        }

        public async Task Delete(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand(sqlQuery, _connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new SqlDalException($"Error executing SQL", ex);
            }
        }

        public async Task<int> Execute(string sqlQuery, params MySqlParameter[] parameters)
        {
            try
            {
                using (var cmd = new MySqlCommand(sqlQuery, _connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new SqlDalException($"Error executing SQL", ex);
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}