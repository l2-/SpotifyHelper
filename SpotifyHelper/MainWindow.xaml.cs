using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpotifyHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string clientID = "c75432186fb643249768f03ad1724b25";
        public static MainWindow mainWindow;
        public SpotifyHelper.Scenes.TextToSpotify textToSpotify;
        public SpotifyHelper.Scenes.MainWindowScene mainWindowScene;
        public SpotifyHelper.Scenes.SpotifyList spotifyList;

        public MainWindow()
        {
            mainWindow = this;
            textToSpotify = new Scenes.TextToSpotify();
            mainWindowScene = new Scenes.MainWindowScene();
            spotifyList = new Scenes.SpotifyList();
            Content = mainWindowScene;
            InitializeComponent();
        }
    }
}
