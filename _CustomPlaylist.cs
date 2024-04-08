using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace NHMPh_music_player
{
    internal class _CustomPlaylist
    {
        MainWindow mainWindow;
        SongsManager songManager;
        MediaPlayer mediaPlayer;
        public static string currentCustomPlayList;
        List<CustomPlaylist> activeWindow = new List<CustomPlaylist>();
        public _CustomPlaylist(MainWindow mainWindow, SongsManager songsManager, MediaPlayer mediaPlayer) 
        {
            this.mainWindow = mainWindow;
            this.songManager = songsManager;
            this.mediaPlayer = mediaPlayer;
            this.mainWindow.loadCustom_btn.Click += LoadCustom_btn_Click;
            this.mainWindow.viewCustom_btn.Click += ViewCustom_btn_Click;
            this.mainWindow.comboboxCustomPlayList.SelectionChanged += ComboboxCustomPlayList_SelectionChanged;
            LoadCustomPlayList();
        }

        private void ComboboxCustomPlayList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            if (text == "CREATE NEW PLAYLIST")
            {

                string[] fileNames = Directory.GetFiles(".\\custom\\");
                int count = 0;
                foreach (string fileName in fileNames)
                {

                    if (System.IO.Path.GetFileNameWithoutExtension(fileName).Contains("New playlist")) count++;
                }
                var data = new JObject(
                new JProperty("thumbnail", "https://i.ytimg.com/vi/J3pF2jkQ4vc/hq720.jpg?sqp=-oaymwEcCOgCEMoBSFXyq4qpAw4IARUAAIhCGAFwAcABBg==&rs=AOn4CLB2BGz1gQ9O8LD0Y4NcWdEfaYgAyw"),
                 new JProperty("title", "New playlist"),
                 new JProperty("songs", new JArray())
                                      );
                if (count == 0)
                {
                    System.IO.File.WriteAllText(".\\custom\\New playlist.json", data.ToString());

                    mainWindow.customPlname.Text = "New playlist";
                    currentCustomPlayList = "New playlist";
                }
                else
                {
                    System.IO.File.WriteAllText($".\\custom\\New playlist ({count}).json", data.ToString());
                    mainWindow.customPlname.Text = $"New playlist ({count})";
                    currentCustomPlayList = $"New playlist ({count})";
                }
                OpenNewWindow();
                LoadCustomPlayList();
                return;

            }

            mainWindow.customPlname.Text = text;
            currentCustomPlayList = text;
        }
        private void OpenNewWindow()
        {
            CustomPlaylist customPlaylistWindow = new CustomPlaylist();
            for (int i = 0; i < activeWindow.Count; i++)
            {
                if (activeWindow[i].Uid == currentCustomPlayList)
                {
                    activeWindow[i].Focus();
                    return;
                }
            }
            customPlaylistWindow.Uid = currentCustomPlayList;
            customPlaylistWindow.Show();
            customPlaylistWindow.OnHeadertextChange += LoadCustomPlayList;
            activeWindow.Add(customPlaylistWindow);
            customPlaylistWindow.Closing += RemoveActiveWindow;
        }
        private void LoadCustomPlayList(object sender, EventArgs e)
        {



            mainWindow.comboboxCustomPlayList.SelectionChanged -= ComboboxCustomPlayList_SelectionChanged;
            mainWindow.comboboxCustomPlayList.Items.Clear();

            // Get all file names in the folder
            string[] fileNames = Directory.GetFiles(".\\custom\\");

            if (fileNames.Length != 0)
            {
                if (currentCustomPlayList == null)
                {
                    currentCustomPlayList = System.IO.Path.GetFileNameWithoutExtension(fileNames[0]);
                }
                activeWindow.Remove(sender as CustomPlaylist);
                CustomPlaylist temp = sender as CustomPlaylist;
                temp.Uid = currentCustomPlayList;
                activeWindow.Add(temp);
                mainWindow.customPlname.Text = currentCustomPlayList;
                // Print all file names
                foreach (string fileName in fileNames)
                {

                    ComboBoxItem comboBoxItem = new ComboBoxItem()
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),
                        Content = System.IO.Path.GetFileNameWithoutExtension(fileName)
                    };
                    mainWindow.comboboxCustomPlayList.Items.Add(comboBoxItem);
                }

            }
            else
            {
                mainWindow.customPlname.Text = "Click here";
            }

            ComboBoxItem _comboBoxItem = new ComboBoxItem()
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                BorderThickness = new Thickness(0),
                Content = "CREATE NEW PLAYLIST"
            };
            mainWindow.comboboxCustomPlayList.Items.Add(_comboBoxItem);
            mainWindow.comboboxCustomPlayList.SelectionChanged += ComboboxCustomPlayList_SelectionChanged;
        }
        private void LoadCustomPlayList()
        {
           mainWindow.comboboxCustomPlayList.SelectionChanged -= ComboboxCustomPlayList_SelectionChanged;
            mainWindow.comboboxCustomPlayList.Items.Clear();

            // Get all file names in the folder
            string[] fileNames = Directory.GetFiles(".\\custom\\");
            if (fileNames.Count() != 0)
            {
                foreach (string fileName in fileNames)
                {

                    ComboBoxItem comboBoxItem = new ComboBoxItem()
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),
                        Content = System.IO.Path.GetFileNameWithoutExtension(fileName)
                    };
                    mainWindow.comboboxCustomPlayList.Items.Add(comboBoxItem);
                }
                /*  currentCustomPlayList = Path.GetFileNameWithoutExtension(fileNames[0]);
                  customPlname.Text = currentCustomPlayList;*/
            }
            // Print all file names


            ComboBoxItem _comboBoxItem = new ComboBoxItem()
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                BorderThickness = new Thickness(0),
                Content = "CREATE NEW PLAYLIST"
            };
            mainWindow.comboboxCustomPlayList.Items.Add(_comboBoxItem);
            mainWindow.comboboxCustomPlayList.SelectionChanged += ComboboxCustomPlayList_SelectionChanged;
        }
        private void RemoveActiveWindow(object sender, CancelEventArgs e)
        {
            activeWindow.Remove(sender as CustomPlaylist);
        }
        private void ViewCustom_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenNewWindow();
        }

        private void LoadCustom_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var playlists = StringUtilitiy.ReadJsonFile($".\\custom\\{currentCustomPlayList}.json");
            Console.WriteLine( playlists["songs"][0]);
            if (playlists == null) return;
            for (int i = 0; i < playlists["songs"].Count(); i++)
            {
               songManager.AddSong(new VideoInfo(playlists["songs"][i]["title"].ToString(), "Song from your custom playlist", playlists["songs"][i]["url"].ToString(), playlists["songs"][i]["thumbnail"].ToString()));
            }
            if (mediaPlayer.PlaybackState == PlaybackState.Stopped)
            {
                songManager.NextSong();
                mediaPlayer.CurrentSong = songManager.CurrentSong;
                Console.WriteLine(mediaPlayer.CurrentSong);
                mediaPlayer.PlayMusic();

            }
        }
    }
}
