using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using steamcito.ViewModels;

namespace steamcito.Views
{
    public partial class MainWindow : Window
    {
        MainWindowModel _mainWindowModel;

        public MainWindow()
        {
            InitializeComponent();
            
            _mainWindowModel = App.ServiceProvider?.GetRequiredService<MainWindowModel>() 
                              ?? throw new InvalidOperationException("ServiceProvider is not initialized");
                              
            LibraryViewControl.Visibility = Visibility.Visible;
            SettingsViewControl.Visibility = Visibility.Collapsed;
            DataContext = _mainWindowModel;
            
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => SystemCommands.CloseWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (s, e) => SystemCommands.MinimizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (s, e) => 
            {
                if (WindowState == WindowState.Maximized)
                    SystemCommands.RestoreWindow(this);
                else
                    SystemCommands.MaximizeWindow(this);
            }));
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowModel.AddGameCommand.Execute(null);
        }

        private void ScanSteam_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowModel.ScanSteamGamesCommand.Execute(null);
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
    }
}