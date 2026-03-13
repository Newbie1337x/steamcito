using Microsoft.EntityFrameworkCore;
using steamcito.Models;

namespace steamcito.Data;

public class AppDBContext : DbContext
{
   
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePaths> GamePaths { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Genre> Genres { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=library_games.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Game>()
            .HasOne(g => g.GamePaths)
            .WithOne()
            .HasForeignKey<GamePaths>(gp => gp.GameId);
        
        modelBuilder.Entity<Game>().OwnsOne(g => g.Details);
        modelBuilder.Entity<Game>().OwnsOne(g => g.Artworks);
    }
}