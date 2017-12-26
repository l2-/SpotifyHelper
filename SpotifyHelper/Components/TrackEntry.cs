using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyHelper.Components
{
    public class TrackEntry : INotifyPropertyChanged
    {
        string artist;
        string album;
        string title;
        string duration;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Artist
        {
            get { return artist; }
            set { artist = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artist")); }
        }
        public string Album
        {
            get { return album; }
            set { album = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Album")); }
        }
        public string Title
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title")); }
        }
        public string Duration
        {
            get { return duration; }
            set { duration = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Duration")); }
        }
        public TrackEntry()
        {

        }

        public TrackEntry(string artist, string title)
        {
            Artist = artist;
            Title = title;
        }

        public TrackEntry(string artist, string album, string title, string duration) : this(artist, title)
        {
            Album = album;
            Duration = duration;
        }

        public override string ToString()
        {
            return "Artist: " + Artist + " Album: " + Album + " Title: " + Title + " Duration: " + Duration;
        }
    }
}
