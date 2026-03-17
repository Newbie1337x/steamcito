using System.Windows;
using steamcito.ViewModels;
namespace steamcito.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowModel mainWindowModel;

        public MainWindow()
        {
            InitializeComponent();
            mainWindowModel = new MainWindowModel();
            LibraryViewControl.Visibility = Visibility.Visible;
            DataContext = mainWindowModel;
        }


        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            mainWindowModel.AddGameCommand.Execute(null);
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            if(LibraryViewControl.Visibility == Visibility.Visible) return;
            LibraryViewControl.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if(SettingsViewControl.Visibility == Visibility.Visible) return;
            SettingsViewControl.Visibility = Visibility.Visible;
        }
    }
}