using Microsoft.EntityFrameworkCore;
using pttInfoCrawler.Model;

namespace pttInfoCrawler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<PostInfo> PostInfo { get; set; }
        public DbSet<LineUser> LineUser { get; set; }
        public DbSet<Board> Board { get; set; }
        public DbSet<KeyWord> KeyWord { get; set; }
    }
}