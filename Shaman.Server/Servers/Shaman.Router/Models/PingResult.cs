using System;

namespace Shaman.Router.Models
{
    [Serializable]
    public class PingResult
    {
        public int ResultCode { get; set; }
        public DateTime UtcNow { get; set; }
        public DateTime UpDate { get; set; }
    }
}