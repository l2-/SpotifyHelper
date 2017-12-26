using SpotifyHelper.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for TextToSpotify.xaml
    /// </summary>
    public partial class TextToSpotify : UserControl
    {
        string defaultInfo = "";

        public ObservableCollection<TrackEntry> TrackListFromText { get; set; }

        public TextToSpotify()
        {
            InitializeComponent();
            defaultInfo = (string)GeneralInfo.Content;
            TrackListFromText = new ObservableCollection<TrackEntry>();
            TrackListGrid.ItemsSource = TrackListFromText;
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.Content = MainWindow.mainWindow.mainWindowScene;
        }

        private async void OnParseText(object sender, RoutedEventArgs e)
        {
            string text = TrackTextBox.Text;
            string pattern = SeperatorPattern.Text;
            List<TrackEntry> tracks = await Task.Run(() => new List<TrackEntry>(TrackList(text, pattern)));
            TrackListFromText.Clear();
            foreach (TrackEntry track in tracks)
            {
                TrackListFromText.Add(track);
            }
        }

        private void OnLostTextBoxFocus(object sender, RoutedEventArgs e)
        {
            string text = TrackTextBox.Text;
            int newLineLen = Environment.NewLine.Length;
            int numLines = text.Length - text.Replace(Environment.NewLine, string.Empty).Length;
            if (newLineLen != 0)
            {
                numLines /= newLineLen;
                numLines++;
            }
            GeneralInfo.Content = defaultInfo + " - Lines: " + numLines;
        }

        public List<TrackEntry> TrackList(string data, string pattern)
        {
            List<TrackEntry> temp = new List<TrackEntry>();
            string _pattern = Regex.Escape(pattern);
            using (StringReader sr = new StringReader(data))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] split = Regex.Split(line, _pattern);//line.Split(pattern.ToCharArray(), StringSplitOptions.None);
                    if (split.Length < 2)
                    {
                        continue;
                    }
                    string artist = split[0];
                    string title = "";
                    for (int i = 1; i < split.Length; i++)
                    {
                        title += split[i];
                    }
                    temp.Add(new TrackEntry(artist, title));
                }
            }
            return temp;
        }

        private void ProcessList(object sender, RoutedEventArgs e)
        {
            if (TrackListFromText == null || TrackListFromText.Count <= 0)
            {
                string text = TrackTextBox.Text;
                string pattern = SeperatorPattern.Text;
                TrackListFromText = new ObservableCollection<TrackEntry>(TrackList(text, pattern));
                TrackListGrid.ItemsSource = TrackListFromText;
            }
            if (TrackListFromText == null || TrackListFromText.Count <= 0)
            {
                MessageBox.Show("Nothing to look-up");
                return;
            }
            if (MainWindow.mainWindow.mainWindowScene._spotify == null)
            {
                MessageBox.Show("You need to authenticate before the client can look up the tracks");
                return;
            }
            if (TrackListFromText.Count <= 0)
            {
                MessageBox.Show("Make sure to have atleast 1 song in the list");
                return;
            }
            MainWindow.mainWindow.Content = MainWindow.mainWindow.spotifyList;
        }

        private async void RemoveFormatting_Click(object sender, RoutedEventArgs e)
        {
            string pattern = "<a.{0,}>.{0,}<\\/a>";
            string text = TrackTextBox.Text;
            //await Task.Run(() =>
            //{
            //    string result = Regex.Replace(text, pattern, string.Empty);//, RegexOptions.Singleline);
            //    string _result ="";
            //    using (StringReader sr = new StringReader(result))
            //    {
            //        string line;
            //        while ((line = sr.ReadLine()) != null)
            //        {
            //            _result += line.Trim(' ') + "\r\n";
            //        }
            //    }
            //    if (_result != null && !string.IsNullOrEmpty(_result))
            //    {
            //        TrackTextBox.Dispatcher.Invoke(() => TrackTextBox.Text = _result);
            //    }
            //});
            string result = await Task.Run(() =>
            {
                return Sanitize(text, pattern);
            });
            if (result != null && !string.IsNullOrEmpty(result))
            {
                TrackTextBox.Dispatcher.Invoke(() => TrackTextBox.Text = result);
            }
        }

        private async void RemoveTimestamps_Click(object sender, RoutedEventArgs e)
        {
            string text = TrackTextBox.Text;
            string pattern = "([0-9]{0,2}:[0-9]{0,2}){1,3}";
            string result = await Task.Run(() =>
            {
                return Sanitize(text, pattern);
            });
            if (result != null && !string.IsNullOrEmpty(result))
            {
                TrackTextBox.Dispatcher.Invoke(() => TrackTextBox.Text = result);
            }
        }

        private string Sanitize(string input, string pattern)
        {
            string result;
            result = Regex.Replace(input, pattern, string.Empty);//, RegexOptions.Singleline);
            string _result = "";
            using (StringReader sr = new StringReader(result))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    _result += line.Trim(' ') + (sr.Peek() != -1 ? "\r\n" : "");
                }
            }
            return _result;
        }
    }
}
