namespace Shaman.Monkeys.Slack
{
    public class SlackMessage
    {
        public string ChannelId { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
    }
}