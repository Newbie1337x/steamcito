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
            DataContext = mainWindowModel;
        }

   
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            mainWindowModel.AddGameCommand.Execute(null);
        }
    }
}