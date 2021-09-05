using Newtonsoft.Json;
using pttInfoCrawler.Enums;

namespace pttInfoCrawler.Model
{
    public class Message
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("type")]
        public LineMessageType type { get; set; }
        [JsonProperty("text")]
        public string text { get; set; }
    }
}