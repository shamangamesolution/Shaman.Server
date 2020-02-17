using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shaman.Monkeys.Slack
{
    public interface ISlackMessageSender
    {
        Task Send(string channelId, string message);
    }

    public class SlackMessageSender : ISlackMessageSender
    {
        private readonly string _token;

        public SlackMessageSender(string token)
        {
            this._token = token;
        }

        public async Task Send(string channelId, string message)
        {
            using (var httpClient = new HttpClient())
            {
                var content = BuildContent(channelId, message);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var res = await httpClient.PostAsync("https://slack.com/api/chat.postMessage", content);

                if (!res.IsSuccessStatusCode)
                {
                    throw new SlackException(
                        $"Error sending slack message: statusCode: {res.StatusCode}, message: {res.Content.ReadAsStringAsync()}");
                }

                Console.Out.WriteLine("res = {0}", await res.Content.ReadAsStringAsync());
            }
        }

        private static StringContent BuildContent(string channelId, string message)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new
                {
                    channel = channelId,
                    text = message
                }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }
    }
}