using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Shaman.DAL.MySQL
{
    public class SqlDal
     {
         private MySqlConnection connection;
         private readonly string server;
         private readonly string database;
         private readonly string uid;
         private readonly string password;

         private readonly Action<string> _logError;
         
         //Constructor
         public SqlDal(string dbServer, string dbName, string dbUser, string dbPassword, int maxPoolSize, Action<string> logError)
         {
             _logError = logError;
             
             server = dbServer;
             database = dbName;
             uid = dbUser;
             password = dbPassword;
 
             Initialize(maxPoolSize);
         }
 
         //Initialize values
         private void Initialize(int maxPoolSize)
         {
             connection = new MySqlConnection(
                 $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};Max Pool Size={maxPoolSize};");
         }
 
         //open connection to database
         private async Task<bool> OpenConnection()
         {
             try
             {
                 await connection.OpenAsync();
                 return true;
             }
             catch (MySqlException ex)
             {
                 _logError($"Open Connection error: {ex}");
                 //When handling errors, you can your application's response based 
                 //on the error number.
                 //The two most common error numbers when connecting are as follows:
                 //0: Cannot connect to server.
                 //1045: Invalid user name and/or password.
                 switch (ex.Number)
                 {
                     case 0:
                         throw new Exception("Cannot connect to server.  Contact administrator", ex);
                     case 1045:
                         throw new Exception("Invalid username/password, please try again", ex);
                     default:
                         throw new Exception("General SQL error", ex);
                 }
             }
         }
 
         //Close connection
         private async Task<bool> CloseConnection()
         {
             try
             {
                 await connection.CloseAsync();
                 return true;
             }
             catch (MySqlException ex)
             {
                 _logError($"Close Connection error: {ex}");
                 throw new Exception("Connection close error", ex);
             }
         }
    
         public async Task<DataTable> Select(string sqlQuery)
         {
             MySqlDataAdapter da = new MySqlDataAdapter();
             DataTable dt = new DataTable();
             try
             {
                 await OpenConnection();
 
                 using (var cmd = new MySqlCommand(sqlQuery, connection))
                 {
                     cmd.CommandType = CommandType.Text;
                     da.SelectCommand = cmd;
                     await da.FillAsync(dt);
                 }
             }
             catch (Exception ex)
             {
                 _logError($"Error executing SQL: {ex}");
                 throw new Exception($"Error executing SQL", ex);
             }
             finally
             {
                 da.Dispose();
                 await CloseConnection();
             }
             return dt;
         }
     
         public async Task<long> Insert(string sqlQuery)
         {
             DataTable dt = new DataTable();
             long lastInsertedId = 0;
             try
             {
                 await OpenConnection();
 
                 using (var cmd = new MySqlCommand(sqlQuery, connection))
                 {
                     cmd.CommandType = CommandType.Text;
                     await cmd.ExecuteNonQueryAsync();
                     lastInsertedId = cmd.LastInsertedId;
                 }
             }
             catch (Exception ex)
             {
                 _logError($"Error executing SQL: {ex}");
                 throw new Exception($"Error executing SQL", ex);
             }
             finally
             {
                 await CloseConnection();
             }
 
             return lastInsertedId;
         }
 
         public async Task Update(string sqlQuery)
         {
             DataTable dt = new DataTable();
             try
             {
                 await OpenConnection();
 
                 using (var cmd = new MySqlCommand(sqlQuery, connection))
                 {
                     cmd.CommandType = CommandType.Text;
                     await cmd.ExecuteNonQueryAsync();
                 }
             }
             catch (Exception ex)
             {
                 _logError($"Error executing SQL: {ex}");
                 throw new Exception($"Error executing SQL", ex);
             }
             finally
             {
                 await CloseConnection();
             }
 
         }
    
         public async Task Delete(string sqlQuery)
         {
             DataTable dt = new DataTable();
             try
             {
                 await OpenConnection();
 
                 using (var cmd = new MySqlCommand(sqlQuery, connection))
                 {
                     cmd.CommandType = CommandType.Text;
                     await cmd.ExecuteNonQueryAsync();
                 }
             }
             catch (Exception ex)
             {
                 _logError($"Error executing SQL: {ex}");
                 throw new Exception($"Error executing SQL", ex);
             }
             finally
             {
                 await CloseConnection();
             }
 
         }
         
          public async Task<int> Execute(string sqlQuery, MySqlParameter[] parameters = null)
          {
              try
              {
                  await OpenConnection();

                  using (var cmd = new MySqlCommand(sqlQuery, connection))
                  {
                      cmd.CommandType = CommandType.Text;
                      if (parameters != null)
                      {
                          cmd.Parameters.AddRange(parameters);
                      }
                      return await cmd.ExecuteNonQueryAsync();
                  }
              }
              catch (Exception ex)
              {
                  _logError($"Error executing SQL: {ex}");
                  throw new Exception($"Error executing SQL", ex);
              }
              finally
              {
                  await CloseConnection();
              }
          }


     }
}
