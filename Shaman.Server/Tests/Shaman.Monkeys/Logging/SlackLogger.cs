using System;
using System.Threading.Tasks;
using Shaman.Monkeys.Slack;

namespace Shaman.Monkeys.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly string _prefix;

        public ConsoleLogger(string prefix)
        {
            _prefix = prefix;
        }

        public void Log(string message)
        {
            Console.WriteLine($"{_prefix}: {message}");
        }
    }

    public class SlackLogger : ILogger
    {
        private readonly string _prefix;
        private readonly string _channelId;
        private readonly SlackMessageSender _slackSender;

        public SlackLogger(string prefix, string channelId, string slackToken)
        {
            _prefix = prefix;
            _channelId = channelId;
            _slackSender = new SlackMessageSender(slackToken);
        }

        public void Log(string message)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await _slackSender.Send(_channelId, $"{_prefix}: {message}");
                }
                catch (Exception e)
                {
                    // ignored
                    Console.WriteLine($"Slack message sending exception: {e}");
                }
            });
        }
    }
}