using System.ComponentModel;
using steamcito.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace steamcito.Views
{
    /// <summary>
    /// Lógica de interacción para LibraryView.xaml
    /// </summary>
    public partial class LibraryView
    {
        public LibraryView()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this) && App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<LibraryViewModel>();
            }
        }
    }
}

