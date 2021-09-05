using Newtonsoft.Json;
using pttInfoCrawler.Enums;

namespace pttInfoCrawler.Model
{
    public class Source
    {
        [JsonProperty("type")]
        public WebhookEventSource type { get; set; }
        [JsonProperty("userId")]
        public string userId { get; set; }
    }
}