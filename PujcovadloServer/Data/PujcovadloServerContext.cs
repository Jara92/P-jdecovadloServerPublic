using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;

namespace PujcovadloServer.Data
{
    public class PujcovadloServerContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public PujcovadloServerContext(DbContextOptions<PujcovadloServerContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Item { get; set; } = default!;
        public DbSet<ItemCategory> ItemCategory { get; set; } = default!;
        public DbSet<Loan> Loan { get; set; } = default!;
        // public DbSet<User> User { get; set; } = default!;

        public DbSet<ItemTag> ItemTag { get; set; } = default!;

        public DbSet<Image> Image { get; set; } = default!;

        public DbSet<PickupProtocol> PickupProtocol { get; set; } = default!;

        public DbSet<ReturnProtocol> ReturnProtocol { get; set; } = default!;

        public DbSet<Review> Review { get; set; } = default!;

        public DbSet<Profile> Profile { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
            /*modelBuilder.Entity<Item>()
                .HasMany<ItemCategory>(i => i.Categories)
                .WithMany(c => c.Items);*/

            // Make tags name unique
            modelBuilder.Entity<ItemTag>()
                .HasIndex(u => u.Name)
                .IsUnique();

            // User has max one profile
            modelBuilder.Entity<Profile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Add Loan.PickupProtocolId foreign key
            modelBuilder.Entity<Loan>()
                .HasOne<PickupProtocol>(l => l.PickupProtocol)
                .WithOne(p => p.Loan)
                .HasForeignKey<Loan>(p => p.PickupProtocolId);

            // Add Loan.ReturnProtocolId foreign key
            modelBuilder.Entity<Loan>()
                .HasOne<ReturnProtocol>(l => l.ReturnProtocol)
                .WithOne(p => p.Loan)
                .HasForeignKey<Loan>(p => p.ReturnProtocolId);

            // Add Image.PickupProtocolId foreign key
            modelBuilder.Entity<Image>()
                .HasOne<PickupProtocol>(i => i.PickupProtocol)
                .WithMany(p => p.Images);

            // Add Image.ReturnProtocolId foreign key
            modelBuilder.Entity<Image>()
                .HasOne<ReturnProtocol>(i => i.ReturnProtocol)
                .WithMany(p => p.Images);

            // Add item main image foreign key
            modelBuilder.Entity<Item>()
                .HasOne(i => i.MainImage)
                .WithOne()
                .HasForeignKey<Item>(i => i.MainImageId)
                .OnDelete(DeleteBehavior
                    .SetNull); // Optional: SetNull means if the main image is deleted, set the MainImageId to null

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