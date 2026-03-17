using System.Windows;
using System.Windows.Input;
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
            SettingsViewControl.Visibility = Visibility.Collapsed;
            DataContext = mainWindowModel;
        }


        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            mainWindowModel.AddGameCommand.Execute(null);
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            if(LibraryViewControl.Visibility == Visibility.Visible) return;
            SettingsViewControl.Visibility = Visibility.Collapsed;
            LibraryViewControl.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if(SettingsViewControl.Visibility == Visibility.Visible) return;
            LibraryViewControl.Visibility = Visibility.Collapsed;
            SettingsViewControl.Visibility = Visibility.Visible;
        }
        
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }
    }
}