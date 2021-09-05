using Newtonsoft.Json;

namespace pttInfoCrawler.Model
{
    public class Event
    {
        [JsonProperty("type")]
        public WebhookEventType type { get; set; }
        [JsonProperty("replyToken")]
        public string replyToken { get; set; }
        [JsonProperty("source")]
        public Source source { get; set; }
        [JsonProperty("timestamp")]
        public long timestamp { get; set; }
        [JsonProperty("mode")]
        public string mode { get; set; }
        [JsonProperty("message")]
        public Message message { get; set; }
    }
}