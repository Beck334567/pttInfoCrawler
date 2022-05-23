namespace pttInfoCrawler.Model
{
    public class KeyWord
    {
        public int KeyWordID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public int BoardID { get; set; }
        public bool IsEncluded { get; set; }
        public bool IsSelectAll { get; set; }
    }
}