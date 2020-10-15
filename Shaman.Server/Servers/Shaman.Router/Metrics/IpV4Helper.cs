using System.Net;

namespace Shaman.Router.Metrics
{
    public static class IpV4Helper
    {
        public static string Get20BitMaskAsString(IPAddress ip)
        {
            var addressBytes = ip.GetAddressBytes();
            addressBytes[2] = (byte) (addressBytes[2] & 0xF0);
            return $"{addressBytes[0]}.{addressBytes[1]}.{addressBytes[2]}.0/20"; 
        }
    }
}