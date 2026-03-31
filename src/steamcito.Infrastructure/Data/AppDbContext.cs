using Microsoft.EntityFrameworkCore;
using steamcito.Models;

namespace steamcito.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }

    public DbSet<Game>          Games          { get; set; }
    public DbSet<GamePaths>     GamePaths      { get; set; }
    public DbSet<GameDetails>   GameDetails    { get; set; }
    public DbSet<Artwork>       Artworks       { get; set; }
    public DbSet<User>          Users          { get; set; }
    public DbSet<Genre>         Genres         { get; set; }
    public DbSet<GameDll>       GameDlls       { get; set; }
    public DbSet<GameDllConfig> GameDllConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>().ToTable("games");
        modelBuilder.Entity<GamePaths>().ToTable("gamepaths");
        modelBuilder.Entity<GameDetails>().ToTable("gamedetails");
        modelBuilder.Entity<Artwork>().ToTable("artworks");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Genre>().ToTable("genres");
        modelBuilder.Entity<GameDll>().ToTable("gamedlls");
        modelBuilder.Entity<GameDllConfig>().ToTable("gamedllconfigs");

        modelBuilder.Entity<GamePaths>()
            .HasMany(gp => gp.Dlls)
            .WithOne(dll => dll.GamePaths)
            .HasForeignKey(dll => dll.GamePathsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameDll>()
            .HasMany(d => d.Configs)
            .WithOne(c => c.GameDll)
            .HasForeignKey(c => c.GameDllId)
            .OnDelete(DeleteBehavior.Cascade);
        
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
        
        modelBuilder.Entity<Game>()
            .Ignore(g => g.IsRunning);
    }
}