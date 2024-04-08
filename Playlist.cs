using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace NHMPh_music_player
{
    internal class Playlist
    {
        MainWindow mainWindow;
        SongsManager songsManager;
        public Playlist(MainWindow mainWindow, SongsManager songsManager) 
        {
            this.mainWindow = mainWindow;
            this.songsManager = songsManager;
            this.songsManager.OnVideoQueueChange += CreatePlaylistCards;
        }

        private void CreatePlaylistCards(object sender, EventArgs e)
        {
            mainWindow.combobox.SelectionChanged -= Combobox_SelectionChanged;
            mainWindow.combobox.Items.Clear();
            if (songsManager.TotalSongInQueue() > 0)
            {


                //Add videoQueue 1st
                int j = 0;
                for (int i = 0; i < songsManager.VideoInfosQueue.Count; i++)
                {
                    StackPanel stackPanel = CreatePlaylistSelection(songsManager.VideoInfosQueue.ElementAt(i).Thumbnail, songsManager.VideoInfosQueue.ElementAt(i).Title, j);
                    mainWindow.combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = stackPanel,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),

                    });
                    j++;
                }

                for (int i = 0; i < songsManager.VideoInfosAutoPlayQueue.Count; i++)
                {

                    StackPanel stackPanel = CreatePlaylistSelection(songsManager.VideoInfosAutoPlayQueue.ElementAt(i).Thumbnail, songsManager.VideoInfosAutoPlayQueue.ElementAt(i).Title, j);
                    mainWindow.combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = stackPanel,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),

                    });
                    j++;
                }

               mainWindow.combobox.SelectionChanged += Combobox_SelectionChanged;
  
            }
            else
            {
               mainWindow.combobox.Items.Add(new ComboBoxItem()
                {
                    Content = "Your list is empty. Either queue your song manually or press autoplay button (lower-left) for endless queue.",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                    BorderThickness = new Thickness(0),
                });
            }
        }
        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as StackPanel;
            if (text != null) 
                MoveTrackManager((text.Children[1] as TextBlock).Text);
        }
        private void MoveTrackManager(string track)
        {

            int id = StringUtilitiy.ExtractTrackId(track);
            Console.WriteLine(id);
            if (id < songsManager.VideoInfosQueue.Count)
            {
                var videoTemp = songsManager.VideoInfosQueue.ElementAt(id);
                songsManager.VideoInfosQueue = new Queue<VideoInfo>(songsManager.VideoInfosQueue.Reverse());
                songsManager.VideoInfosQueue.Enqueue(videoTemp);
                songsManager.VideoInfosQueue = new Queue<VideoInfo>(songsManager.VideoInfosQueue.Reverse());
                songsManager.RemoveSong(id+1);
                
            }
            else
            {
                songsManager.VideoInfosQueue = new Queue<VideoInfo>(songsManager.VideoInfosQueue.Reverse());
                songsManager.VideoInfosQueue.Enqueue(songsManager.VideoInfosAutoPlayQueue.ElementAt(id - songsManager.VideoInfosQueue.Count));
                songsManager.VideoInfosQueue = new Queue<VideoInfo>(songsManager.VideoInfosQueue.Reverse());
                songsManager.RemoveSongAutoPlay(id - songsManager.VideoInfosQueue.Count + 1);
            }

            CreatePlaylistCards(this,null);
        }
        private StackPanel CreatePlaylistSelection(string thumbnail, string title, int id)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            Border border = new Border()
            {
                Height = 30,
                Width = 43,
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(5, 1, 5, 1),
                Background = new ImageBrush()
                {
                    Stretch = Stretch.Fill,
                    ImageSource = new BitmapImage(new Uri(thumbnail))
                },
                BorderThickness = new Thickness(0)

            };
            TextBlock textBlock = new TextBlock()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Text = id + ". " + title,
                Width = 400

            };
            Button button = new Button
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                BorderThickness = new Thickness(0),
                Width = 50,
                Margin = new Thickness(7, 0, 0, 0),
                Content = "Remove",
                Foreground = Brushes.White,
                FontSize = 12,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 20
            };
            button.Click += Delete_Btn_Click;
            button.CommandParameter = id;
            stackPanel.Children.Add(border);
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(button);
            return stackPanel;
        }
        private void Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse((sender as Button).CommandParameter.ToString());
            if (id < songsManager.VideoInfosQueue.Count)
            {
                songsManager.RemoveSong(id);
                 
            }
            else
            {
                songsManager.RemoveSongAutoPlay(id - songsManager.VideoInfosQueue.Count);
           
            }
           
        }
    }
}
