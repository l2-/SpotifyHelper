using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SpotifyHelper.Scenes
{
    /// <summary>
    /// Interaction logic for MainWindowScene.xaml
    /// </summary>
    public partial class MainWindowScene : UserControl
    {
        public SpotifyWebAPI _spotify;
        public WebAPIFactory webApiFactory;

        public MainWindowScene()
        {
            InitializeComponent();
            webApiFactory = new WebAPIFactory(
                "http://localhost",
                8000,
                MainWindow.clientID,
                Scope.PlaylistModifyPrivate | Scope.PlaylistReadPrivate | Scope.UserLibraryRead | Scope.PlaylistModifyPublic |
                Scope.UserFollowRead | Scope.UserTopRead | Scope.PlaylistReadCollaborative |
                Scope.UserReadRecentlyPlayed | Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState, 
                TimeSpan.FromSeconds(60));
        }

        private void OnTrackToText(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.Content = MainWindow.mainWindow.textToSpotify;
        }

        private void OnAuthClicked(object sender, RoutedEventArgs e)
        {
            Task.Run(() => RunAuthentication());
        }

        private async void RunAuthentication()
        {
            if (webApiFactory != null && webApiFactory.authentication != null && webApiFactory.authentication.IsServerActive)
            {
                return;
            }
            try
            {
                _spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                if (type == typeof(OperationCanceledException) || type == typeof(TaskCanceledException))
                    return;
                MessageBox.Show(ex.Message);
            }

            if (_spotify == null)
                return;
            Application.Current.Dispatcher.Invoke(() => InitialSetup());
        }

        private void InitialSetup()
        {
            Auth.Visibility = Visibility.Hidden;
            AuthDetails.Visibility = Visibility.Visible;
            PrivateProfile pf = _spotify.GetPrivateProfile();
            string username = pf.DisplayName != null && pf.DisplayName != "" ? pf.DisplayName : pf.Id;
            AuthUser.Content = username;
        }
    }
}
