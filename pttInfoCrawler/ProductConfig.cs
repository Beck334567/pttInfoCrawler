using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace pttInfoCrawler
{
    internal class ProductConfig : IEntityTypeConfiguration<object>
    {
        public void Configure(EntityTypeBuilder<object> builder)
        {
            throw new System.NotImplementedException();
        }
    }
}