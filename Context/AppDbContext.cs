using Microsoft.EntityFrameworkCore;
using FXChangeWebAPI.Domain.Models;
using FXChangeWebAPI.Domain.Common;
using Newtonsoft.Json;

namespace FXChangeWebAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Quote> Quotes { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Quote>()
        //        .Property(q => q.Rates)
        //        .HasConversion(
        //            v => JsonConvert.SerializeObject(v), // Serializar el diccionario a JSON
        //            v => JsonConvert.DeserializeObject<Dictionary<string, decimal>>(v)); // Deserializar el JSON a un diccionario
        //}
    }
}
