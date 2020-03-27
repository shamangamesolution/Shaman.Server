using CommandLine;

namespace Shaman.Monkeys
{
    public class Options
    {
        [Option('r', "router", Default = "https://routerdomain:8080", HelpText = "URI to router")]
        public string RouterUrl { get; set; }

        [Option('v', "client-version", Default = "<no>1", HelpText = "Version of client to use for routing")]
        public string ClientVersion { get; set; }

        [Option('c', "monkey-max-count", Default = 12, HelpText = "Maximum number of monkeys per room")]
        public int MonkeysMaxCount { get; set; }
        [Option('m', "monkey-min-count", Default = 0, HelpText = "Minimum number of monkeys per room (0 - use MaxNumber constantly)")]
        public int MonkeysMinCount { get; set; }

        [Option('d', "monkey-delay", Default = 100,
            HelpText = "Delay in ms between each monkey starting authorization procedure(to avoid server overloading)")]
        public int MonkeysDelay { get; set; }

        [Option('p', "room-play-duration", Default = 60, HelpText = "Duration of one room (in seconds)")]
        public int RoomPlayDuration { get; set; }

        [Option('g', "games-count", Default = 5, HelpText = "Count of to play (0 to play infinite)")]
        public int GamesCount { get; set; }

        [Option('s', "slack-token", HelpText = "Slack token")]
        public string SlackToken { get; set; }

        [Option('n', "slack-channel", HelpText = "Slack channel to send")]
        public string SlackChannel { get; set; }
    }
}