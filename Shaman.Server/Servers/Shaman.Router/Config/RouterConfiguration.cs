using Shaman.DAL.SQL;

namespace Shaman.Router.Config
{
    public class RouterConfiguration
    {
        public string BindToIP { get; set; }
        public int BindToPortHttp { get; set; }
        public int BindToPortHttps { get; set; }
        
        public SqlDbConfig DbConfig  { get; set; }

        public string SlackBotToken { get; set; }
        public string CustomSecret { get; set; }
        public string AllowedSlackChatId { get; set; }
        public string ServerVersion { get; set; }
        public byte Game { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }
        public int DbMaxPoolSize { get; set; }
    }
}