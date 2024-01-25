using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;

namespace PujcovadloServer.Data
{
    public class PujcovadloServerContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public PujcovadloServerContext(DbContextOptions<PujcovadloServerContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Item { get; set; } = default!;
        public DbSet<ItemCategory> ItemCategory { get; set; } = default!;
       // public DbSet<User> User { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
            /*modelBuilder.Entity<Item>()
                .HasMany<ItemCategory>(i => i.Categories)
                .WithMany(c => c.Items);*/
            
            base.OnModelCreating(modelBuilder);
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Lazy loading related data
            // https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}