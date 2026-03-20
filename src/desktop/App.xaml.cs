using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using steamcito.Data;
using steamcito.Services;
using steamcito.ViewModels;

namespace steamcito
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            
            // Database initialization
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                context.Database.EnsureCreated();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlite("Data Source=library_games.db"));

            // Services
            services.AddSingleton<GameService>();
            services.AddSingleton<PathManager>();
            services.AddSingleton<SteamService>();
            services.AddSingleton<GameSessionManager>();

            // ViewModels
            services.AddTransient<MainWindowModel>();
            services.AddTransient<LibraryViewModel>();
        }
    }
}
