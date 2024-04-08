using AngleSharp.Media;
using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
using TagLib.Ape;
using YoutubeDLSharp;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeSearchApi.Net.Models.Youtube;
using YoutubeSearchApi.Net.Services;
using static System.Net.Mime.MediaTypeNames;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for CustomPlaylist.xaml
    /// </summary>
    public partial class CustomPlaylist : Window
    {
        bool isDelete = false;
        public event EventHandler OnHeadertextChange;
        string playlistname = _CustomPlaylist.currentCustomPlayList;
        public CustomPlaylist()
        {
            InitializeComponent();
            this.MouseDown += Window_MouseDown;
            LoadCustomPLayList();
            Closed += CustomPlaylist_Closed;
            Header.LostFocus += Header_LostFocus;
            // Header.MouseLeave += Header_LostFocus; ;
            Header.GotFocus += Header_MouseLeftButtonDown;
        }



        private void Header_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Click");
            saveFakeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF484848"));
            saveFake.Text = "Save";
        }
        private void saveFakeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            searchBar.Focus();
        }

        private void Header_LostFocus(object sender, RoutedEventArgs e)
        {
            saveFakeBorder.Background = null;
            saveFake.Text = null;
            if (Header.Text != playlistname)
            {
                if (Header.Text == "CREATE NEW PLAYLIST")
                {
                    MessageBox.Show("Error: Invald name");
                    Header.Text = playlistname;
                    return;
                }
                Console.WriteLine("chagne");
                System.IO.File.Delete($".\\custom\\{playlistname}.json");
                currentCustomFile["title"] = Header.Text;
                System.IO.File.WriteAllText($".\\custom\\{Header.Text}.json", currentCustomFile.ToString());
                _CustomPlaylist.currentCustomPlayList = Header.Text;
               // Console.WriteLine(MainWindow.currentCustomPlayList);
                playlistname = Header.Text;
                OnHeadertextChange?.Invoke(this, e);
            }

        }

        private void CustomPlaylist_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("1");
            if (isDelete) return;
            Console.WriteLine("12");
            Console.WriteLine(Header.Text != playlistname);
            if (Header.Text != playlistname)
            {
                if (Header.Text == "CREATE NEW PLAYLIST")
                {
                    MessageBox.Show("Error: Invald name");
                    return;
                }
                System.IO.File.Delete($".\\custom\\{playlistname}.json");
                Console.WriteLine("2");
                currentCustomFile["title"] = Header.Text;
                Console.WriteLine("3");
                System.IO.File.WriteAllText($".\\custom\\{Header.Text}.json", currentCustomFile.ToString());
                Console.WriteLine("4");
            }
            Console.WriteLine("5");
        }

        JObject currentCustomFile;
        private void LoadCustomPLayList()
        {
            
            playlist.Children.Clear();
            currentCustomFile = StringUtilitiy.ReadJsonFile($".\\custom\\{playlistname}.json");
            Console.WriteLine(currentCustomFile);
            Header.Text = playlistname.ToString();
            thumbnail.ImageSource = new BitmapImage(new Uri(currentCustomFile["thumbnail"].ToString()));
            songcount.Text = $"{currentCustomFile["songs"].Count()} song{(currentCustomFile["songs"].Count() > 1 ? "s" : "")}";
            if (currentCustomFile["songs"].Count() == 0)
            {
                // Create a TextBlock
                TextBlock textBlock = new TextBlock();
                textBlock.Margin = new System.Windows.Thickness(10, 25, 0, 0);
                textBlock.Foreground = System.Windows.Media.Brushes.White;
                textBlock.FontSize = 21;
                textBlock.Height = 426;
                textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                textBlock.Width = 416;

                // Add Runs and LineBreaks
                textBlock.Text = "                    ===MAUNAL===\n" +
           " You can search for your song  using\n" +
           "keywords, a YouTube link, or a " +
           "YouTube\nplaylist link.\n" +
           "\n" +
           " Make sure your YouTube playlist link " +
           "isn't\n set to private.\n" +
           "\n" +
           " Youtube auto-generated link is NOT\n supported\n" +
           "\n" +
           " You're also able to change your\n" +
           "playlist name clicking on \"New playlist\"\n" +
           "and thumbnail by clicking on any song\n" +
           "in the playlist.";
                selectionPanel.Children.Add(textBlock);
            }
            for (int i = 0; i < currentCustomFile["songs"].Count(); i++)
            {
                Grid grid = new Grid();
                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                Border border = new Border()
                {
                    Height = 36,
                    Width = 65,
                    CornerRadius = new CornerRadius(2),
                    Margin = new Thickness(5),
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri(currentCustomFile["songs"][i]["thumbnail"].ToString()))
                    },
                    BorderThickness = new Thickness(0)

                };
                TextBlock textBlock = new TextBlock()
                {
                    Margin = new Thickness(5, 0, 0, 0),

                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Text = currentCustomFile["songs"][i]["title"].ToString()
                };
                stackPanel.Children.Add(border);
                stackPanel.Children.Add(textBlock);
                grid.Children.Add(stackPanel);
                ComboBox comboBox = new ComboBox()
                {
                    Opacity = 0,
                    Margin = new Thickness(70, 0, 0, 0),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FFFF")),
                };
                comboBox.Items.Add(new ComboBoxItem()
                {
                    Content = "Move up",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282b30")),
                    BorderThickness = new Thickness(0)
                });
                comboBox.Items.Add(new ComboBoxItem()
                {
                    Content = "Move down",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282b30")),
                    BorderThickness = new Thickness(0)
                });
                comboBox.Items.Add(new ComboBoxItem()
                {
                    Content = "Move top",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282b30")),
                    BorderThickness = new Thickness(0)
                });
                comboBox.Items.Add(new ComboBoxItem()
                {
                    Content = "Move bottom",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282b30")),
                    BorderThickness = new Thickness(0)
                });
                comboBox.Items.Add(new ComboBoxItem()
                {
                    Content = "Set thumbnail",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282b30")),
                    BorderThickness = new Thickness(0)
                });
                comboBox.Items.Add(new ComboBoxItem()
                {
                    Content = "Delete",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282b30")),
                    BorderThickness = new Thickness(0)
                });

                comboBox.Uid = i.ToString();
                comboBox.SelectionChanged += ComboBox_SelectionChanged;
                grid.Children.Add(comboBox);
                grid.MouseEnter += Grid_GotMouseCapture;
                grid.MouseLeave += Grid_MouseLeave;
                playlist.Children.Add(grid);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null) return;
            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            switch (text)
            {
                case "Delete":
                    JArray songsArray = (JArray)currentCustomFile["songs"];
                    songsArray.RemoveAt(int.Parse((sender as ComboBox).Uid));
                    break;
                case "Set thumbnail":
                    (sender as ComboBox).SelectedItem = null;
                    currentCustomFile["thumbnail"] = currentCustomFile["songs"][int.Parse((sender as ComboBox).Uid)]["thumbnail"].ToString();
                    thumbnail.ImageSource = new BitmapImage(new Uri(currentCustomFile["thumbnail"].ToString()));

                    break;
                case "Move up":
                    int i = int.Parse((sender as ComboBox).Uid);
                    if (i == 0) return;
                    var temp = currentCustomFile["songs"][i];
                    currentCustomFile["songs"][i] = currentCustomFile["songs"][i - 1];
                    currentCustomFile["songs"][i - 1] = temp;
                    break;
                case "Move down":
                    i = int.Parse((sender as ComboBox).Uid);
                    if (i == currentCustomFile["songs"].Count() - 1) return;
                    temp = currentCustomFile["songs"][i];
                    currentCustomFile["songs"][i] = currentCustomFile["songs"][i + 1];
                    currentCustomFile["songs"][i + 1] = temp;
                    break;
                case "Move top":
                    i = int.Parse((sender as ComboBox).Uid);
                    if (i == 0) return;
                    for (i = i; i > 0; i--)
                    {
                        temp = currentCustomFile["songs"][i];
                        currentCustomFile["songs"][i] = currentCustomFile["songs"][i - 1];
                        currentCustomFile["songs"][i - 1] = temp;
                    }
                    break;
                case "Move bottom":
                    i = int.Parse((sender as ComboBox).Uid);
                    if (i == currentCustomFile["songs"].Count() - 1) return;
                    for (i = i; i < currentCustomFile["songs"].Count()-1; i++)
                    {
                        temp = currentCustomFile["songs"][i];
                        currentCustomFile["songs"][i] = currentCustomFile["songs"][i + 1];
                        currentCustomFile["songs"][i + 1] = temp;
                    }
                    break;
            }
            System.IO.File.WriteAllText($".\\custom\\{playlistname}.json", currentCustomFile.ToString());
            LoadCustomPLayList();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Grid).Background = null;
            Mouse.OverrideCursor = null;
        }

        private void Grid_GotMouseCapture(object sender, MouseEventArgs e)
        {
            (sender as Grid).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F758594"));
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                Console.WriteLine("Enter");
                Search(searchBar.Text);
                searchBar.Text = null;
            }

        }
        private async void Search(string key)
        {
            selectionPanel.Children.Clear();
            int mode = StringUtilitiy.EvaluateKeyWord(key);
            var youtube = new YoutubeClient();
            switch (mode)
            {
                //link
                case 1:
                    try
                    {
                        var videolinkInfo = await youtube.Videos.GetAsync(key);
                        string info = $"{videolinkInfo.Author} - {videolinkInfo.Duration}";
                        StackPanel stackPanel = CreateVideoSelection(videolinkInfo.Thumbnails.FirstOrDefault().Url, videolinkInfo.Title, info, videolinkInfo.Url);
                        selectionPanel.Children.Add(stackPanel);

                    }
                    catch
                    {
                        MessageBox.Show("Error! This youtube link is wrong or not found");

                    }
                    break;
                //playlist
                case 2:
                    try
                    {
                        var videos = await youtube.Playlists.GetVideosAsync(key);
                        foreach (var video in videos)
                        {
                            string info = $"{video.Author} - {video.Duration}";
                            StackPanel stackPanel = CreateVideoSelection(video.Thumbnails.FirstOrDefault().Url, video.Title, info, video.Url);
                            selectionPanel.Children.Add(stackPanel);
                        }
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Error! Auto-generated playlist link is not supported or playlist is private");

                    }
                    break;
                //search
                case 3:
                    using (var httpClient = new HttpClient())
                    {

                        YoutubeSearchClient client = new YoutubeSearchClient(httpClient);


                        var responseObject = await client.SearchAsync(key);

                        foreach (YoutubeVideo video in responseObject.Results)
                        {
                            string info = $"{video.Author} - {video.Duration}";
                            StackPanel stackPanel = CreateVideoSelection(video.ThumbnailUrl, video.Title, info, video.Url);
                            selectionPanel.Children.Add(stackPanel);
                        }
                    }
                    break;
            }
        }
        StackPanel CreateVideoSelection(string thumbnail, string title, string info, string url)
        {
            Border border = new Border()
            {
                Height = 50,
                Width = 96,
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(10, 10, 10, 0),
                Background = new ImageBrush()
                {
                    Stretch = Stretch.Fill,
                    ImageSource = new BitmapImage(new Uri(thumbnail))
                },
                BorderThickness = new Thickness(0)

            };
            StackPanel InfoPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 224
            };
            TextBlock _title = new TextBlock();
            _title.Foreground = Brushes.Cyan;
            _title.FontSize = 16;
            _title.VerticalAlignment = VerticalAlignment.Top;
            _title.Margin = new Thickness(0, 10, 0, 0);
            _title.Text = title;

            TextBlock _info = new TextBlock();
            _info.Foreground = Brushes.White;
            _info.FontSize = 10;
            _info.VerticalAlignment = VerticalAlignment.Top;
            _info.Margin = new Thickness(2, 0, 0, 0);
            _info.Text = info;

            InfoPanel.Children.Add(_title);
            InfoPanel.Children.Add(_info);

            Button button = new Button();
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF484C52"));
            button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5BECFF"));
            button.BorderThickness = new Thickness(1);
            button.Width = 60;
            button.Padding = new Thickness(0, -1.5, 0, 0);
            button.Margin = new Thickness(7, 0, 0, 0);
            button.Content = "Add";
            button.Foreground = Brushes.White;
            button.FontSize = 15;
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Height = 20;
            button.CommandParameter = new VideoInfo(title, "From custom playlist", url, thumbnail);
           
            button.Click += add_btn;
            Style borderStyle = new Style(typeof(Border));
            borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(3)));

            button.Resources.Add(typeof(Border), borderStyle);
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
            };
            stackPanel.Children.Add(border);
            stackPanel.Children.Add(InfoPanel);
            stackPanel.Children.Add(button);

            return stackPanel;
        }

        private void add_btn(object sender, RoutedEventArgs e)
        {
            var videoInfo = (sender as Button).CommandParameter as VideoInfo;
            var data = new JObject(
                new JProperty("url", videoInfo.Url),
               new JProperty("title", videoInfo.Title),
               new JProperty("thumbnail", videoInfo.Thumbnail),
               new JProperty("description", videoInfo.Description)
             );
            ((JArray)currentCustomFile["songs"]).Add(data);

            Console.WriteLine(currentCustomFile["songs"]);
            System.IO.File.WriteAllText($".\\custom\\{playlistname}.json", currentCustomFile.ToString());
            Console.WriteLine("Added");
            LoadCustomPLayList();
        }

        private void close_btn_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void delete_btn_Click(object sender, RoutedEventArgs e)
        {

            isDelete = true;
            System.IO.File.Delete($".\\custom\\{playlistname}.json");
           _CustomPlaylist.currentCustomPlayList = null;
            OnHeadertextChange?.Invoke(this, e);
            this.Close();
        }

        private void shuffle_Button_Click(object sender, RoutedEventArgs e)
        {
            JArray songs = (JArray)currentCustomFile["songs"];
            List<JToken> songList = songs.ToList();
            // Shuffle the list
            Shuffle(songList);
            // Convert the shuffled list back to a JArray
            songs = new JArray(songList);
            currentCustomFile["songs"] = songs;
            System.IO.File.WriteAllText($".\\custom\\{playlistname}.json", currentCustomFile.ToString());
            Console.WriteLine("Written");
            LoadCustomPLayList();
        }
        static void Shuffle<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

