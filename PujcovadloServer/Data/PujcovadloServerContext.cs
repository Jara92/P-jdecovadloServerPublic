using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Entities;

namespace PujcovadloServer.Data
{
    public class PujcovadloServerContext : DbContext
    {
        public PujcovadloServerContext(DbContextOptions<PujcovadloServerContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Item { get; set; } = default!;
        public DbSet<ItemCategory> ItemCategory { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
            /*modelBuilder.Entity<Item>()
                .HasMany<ItemCategory>(i => i.Categories)
                .WithMany(c => c.Items);*/
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Lazy loading related data
            // https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}