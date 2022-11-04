using Microsoft.EntityFrameworkCore;
using serieshue.Models;

namespace serieshue.Models
{
    public class SeriesHueContext : DbContext
    {
        public SeriesHueContext(DbContextOptions<SeriesHueContext> options) : base(options) { }

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Title>().HasMany(e => e.Episodes);
            modelBuilder.Entity<Episode>().HasOne(e => e.Title);

            modelBuilder.Entity<Title>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english",
                p => new { p.PrimaryTitle, p.OriginalTitle })
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");
        }
        #endregion

        public DbSet<Title>? Titles { get; set; }

        public DbSet<Episode>? Episodes { get; set; }
    }
}