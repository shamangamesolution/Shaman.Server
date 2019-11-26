using System;

namespace AG.Common.Slack
{
    public class SlackException : Exception
    {
        public SlackException(string message) : base(message)
        {
        }
    }
}