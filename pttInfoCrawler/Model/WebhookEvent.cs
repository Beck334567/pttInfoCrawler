using Newtonsoft.Json;

namespace pttInfoCrawler.Model
{
    public class WebhookEvent
    {
        [JsonProperty("destination")]
        public string destination { get; set; }

        [JsonProperty("events")]
        public Event[] events { get; set; }
    }
}