using System;
using System.Collections.Generic;

namespace Shaman.Router.Models
{
    [Serializable]
    public class Result
    {
        public int ResultCode { get; set; }

        public string Message { get; set; }
        public Dictionary<string,object> Data { get; set; }
        public string UserId { get; set; }
        //public string Data { get; set; }

        public Result()
        {
            Message = "";

        }

    }
}