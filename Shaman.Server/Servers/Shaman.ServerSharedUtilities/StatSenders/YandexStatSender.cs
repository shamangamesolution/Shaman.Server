using System;
using System.IO;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Yandex.Metrica;

namespace Shaman.ServerSharedUtilities.StatSenders
{
    public class YandexStatSender : IServerStatsSender
    {
        private readonly IShamanLogger _logger;
        
        public YandexStatSender(IShamanLogger logger)
        {
            _logger = logger;
        }

        public void Initialize(string apiKey)
        {
            string folder = "";
            try
            {
                YandexMetricaFolder.SetCurrent(Directory.GetCurrentDirectory());
                folder = YandexMetricaFolder.Current;
                YandexMetrica.Activate(apiKey);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Yandex metrica activation error (folder: {folder}): {ex}");
            }
        }
        
        
        public async Task SendEvent(string eventName, object item)
        {
            string folder = "";
            try
            {
                folder = YandexMetricaFolder.Current;
                //var json = JsonConvert.SerializeObject(item);
                YandexMetrica.ReportEvent(eventName, item);    
            }
            catch (Exception ex)
            {
                _logger?.Error($"Yandex metrica SendEvent error (folder: {folder}: {ex}");
            }
        }

    }
}