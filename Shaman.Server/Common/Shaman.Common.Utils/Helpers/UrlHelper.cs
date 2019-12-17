namespace Shaman.Common.Utils.Helpers
{
    public class UrlHelper
    {
        public static string GetUrl(int httpPort, int httpsPort, string address)
        {
            var protocol = (httpPort > 0) ? "https" : "http";
            var port = (httpsPort > 0) ? httpsPort : httpPort;
            
            return $"{protocol}://{address}:{port}";
        }
    }
}