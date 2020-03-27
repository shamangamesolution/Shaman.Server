using System;

namespace Shaman.Monkeys.Slack
{
    public class SlackException : Exception
    {
        public SlackException(string message) : base(message)
        {
        }
    }
}