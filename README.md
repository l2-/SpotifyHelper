# Spotify Helper

Little .NET project working with WPF.
The idea was to add songs from youtube to spotify with little effort.
For now it only support:
  - Parse text to spotify songs
  - Authentication through spotify so the app can interact with your spotify account
  - Add the songs to your spotify library with a playlist

To do:
Improve UI. (eg. in-app browser for auth, user more spotify data like existing playlists)
Add support for parsing songs from youtube playlist (link).
...Endless possibilities right?

THIS APPLICATION USES JohnnyCrazy's Spotify .NET WEB API https://github.com/JohnnyCrazy/SpotifyAPI-NET
I made small adjustments to the spotify .net api and included those. For this to work you to download the original project and replace the files of which i have a modified version in this repo.

The release folder includes a an already build .exe file in case you don't want to build the project yourself
