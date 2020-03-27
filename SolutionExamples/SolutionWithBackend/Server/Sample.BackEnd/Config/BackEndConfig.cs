namespace Sample.BackEnd.Config
{
    public class BackendConfiguration
    {
        public string BindToIP { get; set; }
        public int BindToPortHttp { get; set; }
        public int BindToPortHttps { get; set; }

                
        public string DbServerStatic { get; set; }    
        public string DbNameStatic { get; set; }
        public string DbUserStatic { get; set; }
        public string DbPasswordStatic { get; set; }  
        
        public string DbServer { get; set; }    
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }         
      
        public string DbServerTemp { get; set; }    
        public string DbNameTemp { get; set; }
        public string DbUserTemp { get; set; }
        public string DbPasswordTemp { get; set; } 
              
        public bool EnableLog { get; set; }
        public string CustomSecret { get; set; }
        public string EditorToken { get; set; }
        
        public int BackEndId { get; set; }
        
        public string RedisConnectionString { get; set; }
        
        public string ServerVersion { get; set; }
        public string SlackBotToken { get; set; }
        public int DbMaxPoolSize { get; set; } = 1000;

    }
}