using Microsoft.EntityFrameworkCore;
using FXChangeWebAPI.Domain.Models;
using FXChangeWebAPI.Domain.Common;

namespace FXChangeWebAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<History> Histories { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<EUR_COP> EURCOPs { get; set; }
        public DbSet<EUR_USD> EURUSDs { get; set; }
        public DbSet<USD_COP> USDCOPs { get; set; }
        public DbSet<USD_CLP> USDCLPs { get; set; }
        public DbSet<USD_MXN> USDMXNs { get; set; }
        public DbSet<USD_BRL> USDRRLs { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
            
        //}
    }
}
