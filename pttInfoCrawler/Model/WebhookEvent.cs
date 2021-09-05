using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
