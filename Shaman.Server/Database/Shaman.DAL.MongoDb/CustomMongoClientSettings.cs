using MongoDB.Driver;

namespace Shaman.DAL.MongoDb
{
    public interface ICustomMongoClientSettings
    {
        MongoClientSettings GetSettings();
        string GetDataBaseName();
    }
    
    public class CustomMongoClientSettings : ICustomMongoClientSettings
    {
        private readonly string _dataBaseName;
        private readonly MongoClientSettings _settings;

        public CustomMongoClientSettings(string dataBaseName, MongoClientSettings settings)
        {
            _dataBaseName = dataBaseName;
            _settings = settings;
        }
        
        public MongoClientSettings GetSettings()
        {
            return _settings;    
        }

        public string GetDataBaseName()
        {
            return _dataBaseName;
        }
    }
}