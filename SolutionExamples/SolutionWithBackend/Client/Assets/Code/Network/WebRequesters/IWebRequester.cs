using System.Threading.Tasks;

namespace Code.Network.WebRequesters
{
    public interface IWebRequester
    {
        void Initialize();
        Task<string> GetStringAsync(string url);
        Task<RequestResult> PostAsync(string url, byte[] postData);
    }
}