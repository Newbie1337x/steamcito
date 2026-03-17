using steamcito.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace steamcito.Views
{
    /// <summary>
    /// Lógica de interacción para LibraryView.xaml
    /// </summary>
    public partial class LibraryView : System.Windows.Controls.UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
            DataContext = new LibraryViewModel();
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is LibraryViewModel vm)
            {
                vm.RunGameCommand.Execute(null);
            }
        }

     
    }
}

