using Microsoft.EntityFrameworkCore;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class AppDbContext : DbContext
{
    private const string ConnectionString = "Data Source=app.db";

    public DbSet<Reise> Reisen { get; set; }
    public DbSet<Programmpunkt> Programmpunkte { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }
}
