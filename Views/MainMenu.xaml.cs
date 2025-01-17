using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Projet_desktop.Models;

namespace Projet_desktop.Views
{
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
        }
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            Database db = new();
            db.ResetDB();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new FarmPage());
        }

        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            Database db = new();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new FarmPage());
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Close();
        }
    }
}
