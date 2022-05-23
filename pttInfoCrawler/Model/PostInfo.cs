using System;
using System.ComponentModel.DataAnnotations;

namespace pttInfoCrawler.Model
{
    public class PostInfo
    {
        [Key]
        public int ID { get; set; }

        public string Title { get; set; }
        public string Board { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public int TweetCount { get; set; }
    }
}