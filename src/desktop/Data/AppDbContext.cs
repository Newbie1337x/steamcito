using Microsoft.EntityFrameworkCore;
using steamcito.Models;

namespace steamcito.Data;

public class AppDBContext : DbContext
{
    public AppDBContext()
    {
        Database.EnsureCreated();
    }
   
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePaths> GamePaths { get; set; }
    public DbSet<GameDetails> GameDetails { get; set; }
    public DbSet<Artwork> Artworks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Genre> Genres { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=library_games.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>().ToTable("games");
        modelBuilder.Entity<GamePaths>().ToTable("gamepaths");
        modelBuilder.Entity<GameDetails>().ToTable("gamedetails");
        modelBuilder.Entity<Artwork>().ToTable("artworks");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Genre>().ToTable("genres");
        
        modelBuilder.Entity<Game>()
            .HasOne(g => g.GamePaths)
            .WithOne(gp => gp.Game)
            .HasForeignKey<GamePaths>(gp => gp.GameId);
        
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Details)
            .WithOne(d => d.Game)
            .HasForeignKey<GameDetails>(d => d.GameId);
        
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Artworks)
            .WithOne(a => a.Game)
            .HasForeignKey<Artwork>(a => a.GameId);
    }
 
}