using Microsoft.EntityFrameworkCore;
using pttInfoCrawler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pttInfoCrawler
{
    public class LineDBContext : DbContext
    {
        public LineDBContext(DbContextOptions options)
       : base(options)
        {
        }

        public DbSet<PttInfo> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("MySchema");

            builder.ApplyConfiguration(new ProductConfig());
            base.OnModelCreating(builder);
        }
    }
}
