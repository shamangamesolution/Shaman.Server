using MongoDB.Driver;

namespace Shaman.DAL.MongoDb
{
    public interface ICustomMongoClientSettings
    {
        MongoClientSettings GetSettings();
        string GetDataBaseName();
        string GetConnectionString();
    }
    
    public class CustomMongoClientSettings : ICustomMongoClientSettings
    {
        private readonly string _dataBaseName;
        private readonly string _connectionString;
        
        private readonly MongoClientSettings _settings;

        public CustomMongoClientSettings(string connectionString, string dataBaseName)
        {
            _connectionString = connectionString;
            _dataBaseName = dataBaseName;
        }
        
        public CustomMongoClientSettings(string userName, string password, string dataBaseName, MongoClientSettings settings)
        {
            _dataBaseName = dataBaseName;
            _settings = settings;
            var credential = MongoCredential.CreateCredential(dataBaseName, userName, password);
            _settings.Credential = credential;
        }
        
        public MongoClientSettings GetSettings()
        {
            return _settings;    
        }

        public string GetDataBaseName()
        {
            return _dataBaseName;
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}