using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pttInfoCrawler.Model
{
    public class PttInfo
    {
        public string title { get; set; }
        public string board { get; set; }
        public string url { get; set; }
        public DateTime date { get; set; }
        public DateTime postTime { get; set; }
        public int tweetCount { get; set; }
    }
}
