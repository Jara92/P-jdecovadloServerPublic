using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Models;

namespace PujcovadloServer.data
{
    public class PujcovadloServerContext : DbContext
    {
        public PujcovadloServerContext(DbContextOptions<PujcovadloServerContext> options)
            : base(options)
        {
        }

        public DbSet<PujcovadloServer.Models.Item> Item { get; set; } = default!;
        public DbSet<PujcovadloServer.Models.ItemCategory> ItemCategory { get; set; } = default!;

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