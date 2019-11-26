using System.Security.Cryptography;
using System.Text;

namespace AG.Common.Slack
{
    public interface ISlackSignatureValidator
    {
        bool IsValid(string timestamp, string signature, string body);
    }

    public class SlackSignatureValidator : ISlackSignatureValidator
    {
        private readonly byte[] secretBytes;

        public SlackSignatureValidator(string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                throw new SlackException("Slack secret string not set");
            }
            secretBytes = Encoding.UTF8.GetBytes(secret);
        }

        public bool IsValid(string timestamp, string signature, string body)
        {
            var content = $"v0:{timestamp}:{body}";

            var hmacsha256 = new HMACSHA256(secretBytes);
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(content));

            var resultSign = HexHelper.A32BytesToHexString(hash);

            return ("v0=" + resultSign) == signature;
        }
    }
}