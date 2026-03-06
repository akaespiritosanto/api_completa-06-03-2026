using Microsoft.EntityFrameworkCore;

namespace criacao_api4.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Band> Bands { get; set; }
    public DbSet<Cd> Cds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Band>().HasKey(b => b.bandId);
        modelBuilder.Entity<Cd>().HasKey(cd => cd.cdId);

        modelBuilder.Entity<Cd>()
            .HasOne<Band>()
            .WithMany()
            .HasForeignKey(cd => cd.bandId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
