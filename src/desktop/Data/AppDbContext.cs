using Microsoft.EntityFrameworkCore;
using steamcito.Models;

namespace steamcito.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<GamePaths> GamePaths { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Usamos SQLite para una base de datos local sencilla
        optionsBuilder.UseSqlite("Data Source=steamcito.db");
    }
}
