using Microsoft.EntityFrameworkCore;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class AppDbContext : DbContext
{
    private const string ConnectionString = "Data Source=app.db";

    public DbSet<Reise> Reisen { get; set; }
    public DbSet<Programmpunkt> Programmpunkte { get; set; }
    public DbSet<Benutzer> Benutzer { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Benutzernamen sind eindeutig, die Rolle steht als Text in der Datenbank.
        modelBuilder.Entity<Benutzer>()
            .HasIndex(b => b.Benutzername)
            .IsUnique();

        modelBuilder.Entity<Benutzer>()
            .Property(b => b.Rolle)
            .HasConversion<string>();

        base.OnModelCreating(modelBuilder);
    }
}
