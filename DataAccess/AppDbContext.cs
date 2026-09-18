using Microsoft.EntityFrameworkCore;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class AppDbContext : DbContext
{
    public DbSet<Reise> Reisen { get; set; }
    public DbSet<Programmpunkt> Programmpunkte { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=app.db");
        base.OnConfiguring(optionsBuilder);
    }
}
