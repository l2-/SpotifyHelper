using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using SpotifyHelper.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for SpotifyList.xaml
    /// </summary>
    public partial class SpotifyList : UserControl
    {
        public List<FullTrack> spotifyTracks;
        public ObservableCollection<TrackEntry> datagridList; //easier than binding FullTrack properties
        ObservableCollection<TrackEntry> textTracks;
        string defaultInfoListOfSpotifySongs;
        string defaultInfoListFromText;
        public SpotifyWebAPI spotify;
        ManualResetEventSlim isProcessing = new ManualResetEventSlim(false);

        public SpotifyList()
        {
            InitializeComponent();
            datagridList = new ObservableCollection<TrackEntry>();
            TrackListGrid_Copy.ItemsSource = datagridList;
            defaultInfoListFromText = ListFromText.Content.ToString();
            defaultInfoListOfSpotifySongs = ListOfSpotifySongs.Content.ToString();
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.Content = MainWindow.mainWindow.textToSpotify;
        }

        public void ProcessList(ObservableCollection<TrackEntry> col)
        {
            spotifyTracks = new List<FullTrack>();
            if (col == null)
            {
                return;
            }
            if (datagridList == null)
            {
                return;
            }
            ManualResetEvent[] resetEvents = new ManualResetEvent[col.Count];
            Application.Current.Dispatcher.InvokeAsync(() => TextListProgress.Visibility = Visibility.Visible);
            Application.Current.Dispatcher.InvokeAsync(() => TextListStatus.Visibility = Visibility.Visible);
            TextListProgress.Dispatcher.Invoke(() => TextListProgress.Value = 0);
            for (int i = 0; i < col.Count; i++)
            {
                TrackEntry entry = col[i];
                resetEvents[i] = new ManualResetEvent(false);
                int index = i;
                Task.Run(() =>
                {
                    try
                    {
                        TextListProgress.Dispatcher.Invoke(() => TextListProgress.Value += 100 / col.Count / 2f);
                        SearchItem track = MainWindow.mainWindow.mainWindowScene._spotify.SearchItems(entry.Title + " - " + entry.Artist, SpotifyAPI.Web.Enums.SearchType.Track);
                        if (track != null && track.Tracks != null && track.Tracks.Items != null && track.Tracks.Items.Count > 0)
                        {
                            FullTrack _track = track.Tracks.Items[0];
                            string artistName = "";
                            foreach (SimpleArtist artist in _track.Artists)
                            {
                                artistName += artist.Name;
                            }
                            artistName.TrimEnd(' ');
                            TimeSpan t = TimeSpan.FromMilliseconds(_track.DurationMs);
                            string duration = string.Format("{0}:{1:D2}", t.Minutes, t.Seconds);
                            TrackEntry tEntry = new TrackEntry(artistName, _track.Album.Name, _track.Name, duration);
                            lock (spotifyTracks)
                            {
                                spotifyTracks.Add(_track);
                            }
                            Application.Current.Dispatcher.Invoke(() => datagridList.Add(tEntry));
                        }
                    }
                    finally
                    {
                        resetEvents[index].Set();
                        TextListProgress.Dispatcher.Invoke(() => TextListProgress.Value += 100 / col.Count / 2f);
                    }
                });
            }
            WaitHandle.WaitAll(resetEvents);
            Application.Current.Dispatcher.InvokeAsync(() => TextListProgress.Visibility = Visibility.Hidden);
            Application.Current.Dispatcher.InvokeAsync(() => TextListStatus.Visibility = Visibility.Hidden);
        }

        private async void TrackListGrid_Loaded(object sender, RoutedEventArgs e)
        {
            spotify = MainWindow.mainWindow.mainWindowScene._spotify;
            if (MainWindow.mainWindow.textToSpotify.TrackListFromText != null)
            {
                if (textTracks != null && textTracks.SequenceEqual(MainWindow.mainWindow.textToSpotify.TrackListFromText))
                {
                    return;
                }

                Application.Current.Dispatcher.Invoke(() => datagridList.Clear());
                textTracks = new ObservableCollection<TrackEntry>(MainWindow.mainWindow.textToSpotify.TrackListFromText);
                TrackListGrid.ItemsSource = textTracks;
                isProcessing.Set();
                await Task.Run(() => ProcessList(textTracks));
                isProcessing.Reset();
                if (spotifyTracks != null)
                {
                    ListOfSpotifySongs.Content = defaultInfoListOfSpotifySongs + " " + spotifyTracks.Count + " tracks";
                    ListFromText.Content = defaultInfoListFromText + " " + textTracks.Count + " tracks";
                }
            }
        }

        private async void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessing.IsSet)
            {
                return;
            }
            if (string.IsNullOrEmpty(PlaylistName.Text))
            {
                MessageBox.Show("Please enter a name for the playlist");
                return;
            }
            if (spotify != null)
            {
                string playlistName = PlaylistName.Text;
                FullPlaylist playlist = await Task.Run(() => spotify.CreatePlaylistAsync(spotify.GetPrivateProfile().Id, playlistName));
                await Task.Run(() => AddToPlaylist(spotify.GetPrivateProfile().Id, playlist, spotifyTracks));
                MessageBox.Show("Playlist created!");
            }
        }

        private async void AddToPlaylist(string userID, FullPlaylist playlist, List<FullTrack> tracks)
        {
            List<string> URIs = new List<string>();
            foreach(FullTrack track in tracks)
            {
                URIs.Add(track.Uri);
            }
            await spotify.AddPlaylistTracksAsync(userID, playlist.Id, URIs);
        }
    }
}
