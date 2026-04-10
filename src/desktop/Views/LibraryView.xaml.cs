using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using steamcito.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Button = System.Windows.Controls.Button;

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
        
        private void MainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double initialTopMargin = 300;
            double newTopPosition = Math.Max(0, initialTopMargin - MainScroll.VerticalOffset);
            
            StickyButtonsPanel.Margin = new Thickness(0, newTopPosition, 0, 0);
        }
    }
}

