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
        public PujcovadloServerContext (DbContextOptions<PujcovadloServerContext> options)
            : base(options)
        {
        }

        public DbSet<PujcovadloServer.Models.Item> Item { get; set; } = default!;
    }
}
