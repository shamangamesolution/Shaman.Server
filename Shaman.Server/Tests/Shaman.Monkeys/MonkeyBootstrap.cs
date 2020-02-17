﻿using System;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using Shaman.Common.Utils.Logging;
using Shaman.Monkeys.Logging;
using Shaman.TestTools.Monkeys;
using ConsoleLogger = Shaman.Monkeys.Logging.ConsoleLogger;

namespace Shaman.Monkeys
{
    public class Options
    {
        [Option('r', "router", Default = "https://***REMOVED***.***REMOVED***.com:6006", HelpText = "URI to router")]
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

    public interface IMonkeyBehaviour
    {
        Task Authenticate(Monkey monkey);
        Task Play(Monkey monkey);

        IMonkeyFactory CreateMonkeyFactory(IShamanLogger logger, Options options);
    }

    public class  MonkeyBootstrap
    {
        public static void Launch(string[] args, IMonkeyBehaviour monkeyBehaviour)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                Execute(options, monkeyBehaviour);
            });
        }

        private static void Execute(Options options, IMonkeyBehaviour monkeyBehaviour)
        {
            RunMonkeys(options, monkeyBehaviour).Wait();
        }

        private static async Task RunMonkeys(Options options, IMonkeyBehaviour monkeyBehaviour)
        {
            var logger = CreateLogger(options);
            logger.Log(
                $"Options: {JsonConvert.SerializeObject(options, Formatting.Indented)}{Environment.NewLine}Monkeys, GO!");
            try
            {
                var monkeyFactory = monkeyBehaviour.CreateMonkeyFactory(new ShamanMonkeyLogger(logger),options); 
                await LaunchMonkeyGroup(options, logger, monkeyFactory, monkeyBehaviour);
            }
            catch (Exception e)
            {
                logger.Log($"Monkeys gone with troubles {e}");
            }
            finally
            {
                logger.Log("Monkeys gone.");
            }
        }

        private static async Task LaunchMonkeyGroup(Options options, ILogger logger, IMonkeyFactory monkeyFactory, IMonkeyBehaviour monkeyBehaviour)
        {
            var random = new Random();
            var monkeysCount = GetMonkeysCount(options, random);
            try
            {
                var monkeyGroup = await monkeyFactory.PrepareMonkeys(monkeysCount, options.MonkeysDelay, monkeyBehaviour.Authenticate);
                
                for (var gameIndex = 0; gameIndex < options.GamesCount || options.GamesCount == 0; gameIndex++)
                {
                    try
                    {
                        var result = await monkeyGroup.SendMonkeys(TimeSpan.FromSeconds(options.RoomPlayDuration), monkeyBehaviour.Play);
                        logger.Log($"Game {gameIndex + 1}:{Environment.NewLine}{result}");
                    }
                    catch (Exception e)
                    {
                        logger.Log($"Error during monkeys play: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log($"Error during preparing monkeys: {e}");
            }
        }

        private static int GetMonkeysCount(Options options, Random random)
        {
            return options.MonkeysMinCount == 0
                ? options.MonkeysMaxCount
                : random.Next(options.MonkeysMinCount, options.MonkeysMaxCount + 1);
        }


        private static ILogger CreateLogger(Options options)
        {
            var prefix = Guid.NewGuid().ToString();
            return string.IsNullOrEmpty(options.SlackToken) || string.IsNullOrEmpty(options.SlackChannel)
                ? (ILogger) new ConsoleLogger(prefix)
                : new CompositeLogger(new ConsoleLogger(prefix),
                    new SlackLogger(prefix, options.SlackChannel, options.SlackToken));
        }
    }
}