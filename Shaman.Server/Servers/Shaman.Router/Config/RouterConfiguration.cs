namespace Shaman.Router.Config
{
    public class RouterConfiguration
    {
        public string BindToIP { get; set; }
        public int BindToPortHttp { get; set; }
        public int BindToPortHttps { get; set; }

        public string DbServer { get; set; }    
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; } 
        public string SlackBotToken { get; set; }
        public string CustomSecret { get; set; }
        public string AllowedSlackChatId { get; set; }
        public string ServerVersion { get; set; }
        public byte Game { get; set; }
    }
}