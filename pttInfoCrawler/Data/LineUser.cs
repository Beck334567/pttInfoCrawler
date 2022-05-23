using System;
using System.ComponentModel.DataAnnotations;

namespace pttInfoCrawler.Model
{
    public class LineUser
    {
        [Key]
        public int UserID { get; set; }

        public string UserName { get; set; }
        public bool Diasbled { get; set; }
        public string LineToken { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Level { get; set; }
    }
}