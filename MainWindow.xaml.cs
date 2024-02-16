using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;
using System.IO;
using System.Net.Http;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeSearchApi.Net.Models.Youtube;
using YoutubeSearchApi.Net.Services;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Common;
using AngleSharp;
using PuppeteerSharp;
using System.Security.Policy;
using System.Text.RegularExpressions;
using YoutubeExplode.Playlists;
using TagLib.Ape;
using System.Collections;
using AngleSharp.Dom.Events;
using System.Windows.Controls;
using System.Windows.Media;
using AngleSharp.Dom;
using YoutubeExplode.Videos;
using Newtonsoft.Json.Linq;


namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isChromeOpen= false;
        WaveOutEvent output = new WaveOutEvent();
        MediaFoundationReader _mf;
        ThreadStart start;
        Thread thread;
        bool isChosingTimeStap;
        double timeInSong;
        bool isLoop;
        bool isAutoPlay;
        string currenturl = string.Empty;
        YoutubeDL ytdl = new YoutubeDL();

        //Normal video info
        Queue<VideoInfo> videoQueue = new Queue<VideoInfo>();
        //Normal video info for autoplay
        Queue<VideoInfo> videoAutoQueue = new Queue<VideoInfo>();
        //real queue for 
        Queue<string> videoPlayListQueue = new Queue<string>();
        Queue<string> videoPlayListQueueTitile = new Queue<string>();


        public MainWindow()
        {

            InitializeComponent();

            this.MouseDown += Window_MouseDown;
            pauseBtn.Style = null;
            Topmost = true;
            Icon = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\icon.ico"));
            start = new ThreadStart(TrackManager);
            thread = new Thread(start);

        }

        private async void PlayMusic(VideoInfo videoInfo)
        {
            if (videoInfo.url == null) return;
            currenturl = videoInfo.url;
            Console.Write(currenturl);
            //Convert url to audiable link
            var options = new OptionSet() { Format = "m4a", GetUrl = true };
            var streamUrl = await ytdl.RunWithOptions(
                new[] { videoInfo.url },
                options,
                CancellationToken.None
            );
            Console.WriteLine("\ndsd " + streamUrl.ErrorOutput.ToString());
            string url = null;

            foreach (var item in streamUrl.Data)
            {
                url = item;
                Console.WriteLine(url);
            }
            Console.WriteLine(url);
            //set _mf for playing audio
            _mf = new MediaFoundationReader(url);
            //play audio
            if (output != null)
                output.Dispose();
            output.Init(_mf);
            output.Play();

            SetVisual("Now playing: " + videoInfo.title, videoInfo.description.Length > 200 ? videoInfo.description.Substring(0, 200) + "..." : videoInfo.description, videoInfo.thumbnail);
            //Start track thread
            if (thread.ThreadState != System.Threading.ThreadState.WaitSleepJoin)
            {
                thread.Start();
            }
            SetTrackVisual();
            CheckQueue();
            CheckNextSong();
        }
        private void SetTrackVisual()
        {
            songProgress.Value = 0;
            thumb.Value = 0;
            songProgress.Maximum = _mf.TotalTime.TotalMilliseconds;
            thumb.Maximum = _mf.TotalTime.TotalMilliseconds;
        }
        private void TrackManager()
        {

            while (true)
            {

                Dispatcher.Invoke(() =>
                {

                    //Song end
                    if (songProgress.Value == songProgress.Maximum)
                    {
                        //When loop is on
                        if (isLoop)
                        {
                            SetTrackVisual();
                            _mf.Seek(0, SeekOrigin.Begin);

                        }

                        else
                        {
                            //When there's song(s) in queue
                            if (videoQueue.Count != 0)
                            {
                                _mf = null;
                                songProgress.Value = 0;
                                Console.WriteLine("VideoQueue");
                                PlayMusic(videoQueue.Dequeue());

                            }
                            //No song in queue but autoplay is on
                            else
                            {
                                if (videoAutoQueue.Count != 0)
                                {
                                    _mf = null;
                                    songProgress.Value = 0;
                                    Console.WriteLine("AutoVideoQueue");
                                    PlayMusic(videoAutoQueue.Dequeue());

                                    songProgress.Value = 0;

                                }
                            }
                        }

                    }
                    else
                    {
                        if (_mf != null)
                            songProgress.Value = _mf.CurrentTime.TotalMilliseconds;
                    }

                });
                Thread.Sleep(1);
            }
        }
        private async void CheckQueue()
        {

            if (videoPlayListQueue.Count == 0)
            {
                if (isAutoPlay)
                {
                    GetPlayList();
                }
                    

            }
            while (videoAutoQueue.Count < 1)
            {

                videoAutoQueue.Enqueue(await Search(videoPlayListQueue.Dequeue()));
                videoPlayListQueueTitile.Dequeue();

            }


            CheckNextSong();
        }
        private async Task<VideoInfo> Search(string key)
        {
            var youtube = new YoutubeClient();
            VideoInfo videoInfo = new VideoInfo();
            int mode = EvaluateKeyWord(key);
            //Search by link
            if (mode == 1)
            {
                try
                {
                    var videolinkInfo = await youtube.Videos.GetAsync(key);
                    videoInfo.title = videolinkInfo.Title;
                    videoInfo.description = videolinkInfo.Description;
                    videoInfo.url = videolinkInfo.Url;
                    videoInfo.thumbnail = videolinkInfo.Thumbnails.FirstOrDefault().Url;
                    return videoInfo;
                }
                catch
                {
                    MessageBox.Show("Error! This youtube link is wrong or not found");
                    return videoInfo;
                }

            }

            if (mode == 2)
            {
                try
                {
                    var videos = await youtube.Playlists.GetVideosAsync(key);
                    for (int i = 1; i < videos.Count; i++)
                    {
                        videoQueue.Enqueue(new VideoInfo()
                        {
                            title = videos[i].Title,
                            thumbnail = videos[i].Thumbnails.FirstOrDefault().Url,
                            url = videos[i].Url,
                            description = "Song by " + videos[i].Author

                        });
                    }
                    videoInfo.title = videos.First().Title;
                    videoInfo.description = $"Play lists with {videos.Count} videos";
                    videoInfo.thumbnail = videos.First().Thumbnails.FirstOrDefault().Url;
                    videoInfo.url = videos.First().Url;
                    return videoInfo;
                }
                catch (Exception ex)
                {
                    
                        MessageBox.Show("Error! Playlist link is wrong or not found");
                        return null;
                    

                }


            }
            Console.WriteLine($"Searching {key}");

            //Search by key
            using (var httpClient = new HttpClient())
            {

                YoutubeSearchClient client = new YoutubeSearchClient(httpClient);


                var responseObject = await client.SearchAsync(key);

                foreach (YoutubeVideo video in responseObject.Results)
                {

                    var description = await youtube.Videos.GetAsync(video.Url);
                    videoInfo.title = video.Title;
                    videoInfo.description = description.Description;
                    videoInfo.url = video.Url;
                    videoInfo.thumbnail = video.ThumbnailUrl;

                    return videoInfo;
                }
                return null;
            }

        }
        private void SetVisual(string _title, string _desciption, string _thumbnail)
        {
            title.Content = _title;
            description.Text = _desciption;
            thumbnail.Source = new BitmapImage(
             new Uri(_thumbnail));
        }    
        private async void GetPlayList()
        {
            isChromeOpen = true;
            IBrowser browser;
            try
            {
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
                });
            }
            catch
            {
                await new BrowserFetcher().DownloadAsync();
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                });
            }
            string videoUrl = $"{currenturl}&list=RD{ExtractId(currenturl)}&themeRefresh=1";
            var page = await browser.NewPageAsync();
            await page.GoToAsync(videoUrl);
            await page.WaitForSelectorAsync("a#wc-endpoint");
            var content = await page.GetContentAsync();
            await browser.CloseAsync();
            isChromeOpen= false;
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(res => res.Content(content));
            var wcEndpointHrefs = document.QuerySelectorAll("a#wc-endpoint")
           .Select(a => a.GetAttribute("href"))
           .ToList();
            var wcEndpointTitles = document.QuerySelectorAll("#video-title")
          .Select(a => a.GetAttribute("title"))
          .ToList();

            Console.WriteLine("Count1: " + wcEndpointHrefs.Count());

            videoPlayListQueueTitile.Clear();
            videoAutoQueue.Clear();
            videoPlayListQueue.Clear();
            int stopCount = 2;


            // Print the extracted href values
            foreach (var href in wcEndpointHrefs)
            {
                if (stopCount == 2)
                {
                    videoPlayListQueue.Enqueue($"https://www.youtube.com/watch?v={ExtractId(href)}");

                    stopCount = 0;

                }
                else
                {
                    stopCount++;
                }
            }
            stopCount = 0;
            foreach (var title in wcEndpointTitles)
            {
                if (title != null && stopCount < 25)
                {
                    videoPlayListQueueTitile.Enqueue(title);
                    stopCount++;
                }
            }

            //remove the original
            videoPlayListQueueTitile.Dequeue();
            videoPlayListQueue.Dequeue();
            CheckQueue();
            CheckNextSong();
        }
        //Call when song change
        void CheckNextSong()
        {
            combobox.SelectionChanged -= Combobox_SelectionChanged;
            combobox.Items.Clear();
            if (videoQueue.Count + videoPlayListQueue.Count > 0)
            {

                int j = 0;
                //Add videoQueue 1st

                for (int i = 0; i < videoQueue.Count; i++)
                {
                    combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = j + ". " + videoQueue.ElementAt(i).title,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),
                    });
                    j++;
                }
                for (int i = 0; i < videoAutoQueue.Count; i++)
                {
                    combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = j + ". " + videoAutoQueue.ElementAt(i).title + " (autoplay)",
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),
                    });
                    j++;
                }
                //Add videoAutoQueue 2nd
                for (int i = 0; i < videoPlayListQueueTitile.Count; i++)
                {
                    combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = j + ". " + videoPlayListQueueTitile.ElementAt(i) + " (autoplay)",
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),
                    });
                    j++;
                }
                combobox.SelectionChanged += Combobox_SelectionChanged;
            }
            else
            {
                combobox.Items.Add(new ComboBoxItem()
                {
                    Content = "Your list is empty. Either queue your song manually or press autoplay button (lower-left) for endless queue.",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                    BorderThickness = new Thickness(0),
                });
            }
            queue_txt.Text = $" {videoQueue.Count + videoPlayListQueue.Count + videoAutoQueue.Count} ";
            next_txt.Text = videoQueue.Count != 0 ? $"{videoQueue.ElementAt(0).title}" : videoAutoQueue.Count != 0 ? $"{videoAutoQueue.ElementAt(0).title}" : $"Click here to see your playlist";
        }
        private async void MoveTrackManager(string track)
        {
            next_txt.Text = RemoveIdAndParentheses(track);
            int id = ExtractTrackId(track);
            if (id < videoQueue.Count)
            {
                var videoTemp = videoQueue.ElementAt(id).url;
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                videoQueue.Enqueue(await Search(videoTemp));
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                RemoveAtIndex(videoQueue, id + 1);

            }
            else if (id == videoQueue.Count && videoAutoQueue.Count != 0)
            {
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                videoQueue.Enqueue(videoAutoQueue.Dequeue());
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                CheckQueue();
            }
            else
            {
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                videoQueue.Enqueue(await Search(videoPlayListQueue.ElementAt(id - 1 - videoQueue.Count)));
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                Console.WriteLine($"Remove at {id - videoQueue.Count + 1 - videoAutoQueue.Count}");
                RemoveAtIndex(videoPlayListQueueTitile, id - videoQueue.Count + 1 - videoAutoQueue.Count);
                RemoveAtIndex(videoPlayListQueue, id - videoQueue.Count + 1 - videoAutoQueue.Count);
            }
            CheckNextSong();
        }   
        
        
        
        //Support function
        int EvaluateKeyWord(string key)
        {
            if (IsYouTubePlaylistLink(key))
            {
                Console.WriteLine("playlink");

                return 2;
            }
            else if (IsYouTubeVideoLink(key))
            {
                Console.WriteLine("link");
                return 1;
            }
            else
            {
                Console.WriteLine("serasdlink");
                return 3;
            }
        }
        bool IsYouTubeVideoLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/watch\?v=|youtu\.be/)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        bool IsYouTubePlaylistLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/playlist\?list=|youtube\.com/watch\?v=.+&list=)([a-zA-Z0-9_-]+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        bool IsYouTubeAutoPlaylistLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/playlist\?list=|youtube\.com/watch\?v=.+&((list=[a-zA-Z0-9_-]+)|list=RD[a-zA-Z0-9_-]+(&start_radio=\d)?))";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        string ExtractId(string href)
        {
            int indexV = href.IndexOf("v=");
            if (indexV != -1)
            {
                // Extract the video ID starting from the index of "v="
                string videoId = href.Substring(indexV + 2);

                // Remove any additional parameters by finding the index of "&"
                int indexAmpersand = videoId.IndexOf("&");

                if (indexAmpersand != -1)
                {
                    videoId = videoId.Substring(0, indexAmpersand);
                }

                return videoId;

            }
            return null;
        }
        static string RemoveIdAndParentheses(string input)
        {
            // Define a regular expression pattern to match the ID and parentheses and content inside parentheses
            string pattern = @"^\d+\.\s|\([^()]*\)";

            // Replace the matched pattern with an empty string
            string result = Regex.Replace(input, pattern, "");

            return result;
        }
        static int ExtractTrackId(string input)
        {
            // Define a regular expression pattern to match the ID number
            string pattern = @"^(\d+)\.";

            // Match the pattern in the input string
            Match match = Regex.Match(input, pattern);

            // Check if the match is successful and extract the ID
            if (match.Success)
            {
                // Extract the ID from the first capturing group
                int id;
                if (int.TryParse(match.Groups[1].Value, out id))
                {
                    return id;
                }
            }

            // Return -1 if ID extraction fails
            return -1;
        }
        void RemoveAtIndex<T>(Queue<T> queue, int index)
        {
            if (index < 0 || index >= queue.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Queue<T> tempQueue = new Queue<T>();

            // Dequeue elements until the desired index
            for (int i = 0; i < index; i++)
            {
                tempQueue.Enqueue(queue.Dequeue());
            }

            // Remove the element at the desired index
            queue.Dequeue();
            while (queue.Count > 0)
            {
                tempQueue.Enqueue(queue.Dequeue());
            }
            // Enqueue back the remaining elements from tempQueue
            while (tempQueue.Count > 0)
            {
                queue.Enqueue(tempQueue.Dequeue());
            }

        }
        public static JObject ReadJsonFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string jsonString = reader.ReadToEnd();
                    return JObject.Parse(jsonString);
                }
            }
            else { Console.WriteLine("NotFOund"); }

            return null;
        }
        //////////////

        //event
        private void volum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            output.Volume = (float)volum.Value;
            volumVisual.Value = volum.Value;
        }
        private void songProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isChosingTimeStap)
            {
                thumb.Value = songProgress.Value;
            }

        }
       
        private void thumb_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (isChosingTimeStap == false)
            {
                timeInSong = thumb.Value;
            }
            isChosingTimeStap = true;
            Console.WriteLine(timeInSong);
        }
        private void thumb_LostMouseCapture(object sender, MouseEventArgs e)
        {

            int second = (int)Math.Floor(thumb.Value / 1000);
            int current = (int)Math.Floor(timeInSong / 1000);
            int secondToSkip = second - current;
            _mf.Skip(secondToSkip);
            songProgress.Value = _mf.CurrentTime.TotalMilliseconds;
            isChosingTimeStap = false;
            if (output.PlaybackState != PlaybackState.Playing)
            {
                output.Play();
            }
        }
        private void thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = Mouse.GetPosition(songProgress);
            Console.WriteLine("Mouse position: X = " + position.X + ", Y = " + position.Y);

            timeInSong = thumb.Value;
            double mousePositionPercentage = position.X / songProgress.Width;
            double thumbPostion = thumb.Maximum * mousePositionPercentage;
            int second = (int)Math.Floor(thumbPostion / 1000);
            int current = (int)Math.Floor(thumb.Value / 1000);
            int secondToSkip = second - current;
            _mf.Skip(secondToSkip);
            songProgress.Value = _mf.CurrentTime.TotalMilliseconds;
            if (output.PlaybackState != PlaybackState.Playing)
            {
                output.Play();
            }

        }
       
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (currenturl == string.Empty) return;
            output.Pause();
            Console.WriteLine(thread.ThreadState);


            playpause.Source = new BitmapImage(
            new Uri($"{Environment.CurrentDirectory}\\Images\\_Play.png"));
            pauseBtn.Click -= Button_Click_1;
            pauseBtn.Click += Button_Click_2;


        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            output.Play();
            playpause.Source = new BitmapImage(
            new Uri($"{Environment.CurrentDirectory}\\Images\\_Pause.png"));
            pauseBtn.Click -= Button_Click_2;
            pauseBtn.Click += Button_Click_1;
        }
        private void Skip_Btn(object sender, RoutedEventArgs e)
        {

            //When there's song(s) in queue
            if (videoQueue.Count > 0)
            {
                PlayMusic(videoQueue.Dequeue());

            }
            else  //No song in queue but autoplay is on
            if (videoQueue.Count == 0 && videoAutoQueue.Count != 0)
            {
                PlayMusic(videoAutoQueue.Dequeue());
            }
        }
        private void loopBtn_Click(object sender, RoutedEventArgs e)
        {
            isLoop = !isLoop;
            loopTxt.Text = isLoop ? " on " : " off ";
            Console.WriteLine(isLoop);
        }
        private void autoplay_btn_Click(object sender, RoutedEventArgs e)
        {
            if (isChromeOpen)
            {
                MessageBox.Show("Task in progress please wait!");
                return;
            }
            if (currenturl == string.Empty)
            {
                MessageBox.Show("Play any song to use autoplay");
                return;
            }
            isAutoPlay = !isAutoPlay;

            if (isAutoPlay)
            {
                auto_txt.Text = " on ";
                if (videoPlayListQueue.Count == 0)
                {
                    GetPlayList();
                }
            }
            else
            {
                auto_txt.Text = " off ";
                //remove later
                videoQueue.Clear();
                videoPlayListQueue.Clear();
                videoAutoQueue.Clear();
                videoPlayListQueueTitile.Clear();
                CheckNextSong();
            }


        }
        private async void Prevous_Btn(object sender, RoutedEventArgs e)
        {
            //Change some day
            /*            if (videoQueue.Count > 0)
                        {


                            videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                            if (videoQueue.Count > 0)
                            {
                                PlayMusic(videoQueue.Dequeue());
                            }
                            if (videoQueue.Count == 0 && videoAutoQueue.Count != 0)
                            {
                                PlayMusic(videoAutoQueue.Dequeue());

                            }
                            videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                        }
                        else
                        {
                            if (videoPlayListQueue2.Count < 0) return;
                            videoPlayListQueue2 = new Queue<string>(videoPlayListQueue2.Reverse());
                            videoPlayListQueue2.Enqueue(videoQueue.Dequeue().url);
                            videoQueue.Enqueue(await Search(videoPlayListQueue2.Dequeue()));
                            if (videoQueue.Count > 0)
                            {
                                PlayMusic(videoQueue.Dequeue());

                            }
                            if (videoQueue.Count == 0 && videoAutoQueue.Count != 0)
                            {
                                PlayMusic(videoAutoQueue.Dequeue());

                            }
                            videoPlayListQueue2 = new Queue<string>(videoPlayListQueue2.Reverse());
                        }*/

        }
       
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Add song to queue
            if (e.Key == Key.Enter)
            {

                string key = searchBar.Text;
                searchBar.Text = null;
                var result = await Search(key);
                if (result == null)
                    return;
                videoQueue.Enqueue(result);

                if (output.PlaybackState == PlaybackState.Stopped)
                    PlayMusic(videoQueue.Dequeue());

            }
            CheckNextSong();
            if (e.Key == Key.T)
            {
                Console.WriteLine($"{videoQueue.Count} {videoAutoQueue.Count} {videoPlayListQueue.Count}");
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();

            //  cts.Cancel();
        }
       
        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            if (text != null)
                Console.WriteLine(text);
            MoveTrackManager(text);
        }
        private void comboboxCustomPlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            if (text != null)
                Console.WriteLine(text);
            LoadCustomPlaylist(text);
        }
        //////////////

        //Custom playlist
        private void LoadCustomPlaylist(string plName)
        {
           var playlists = ReadJsonFile("D:\\WPF\\NHMPh music player\\Light-Music-Player\\bin\\Debug\\custom\\Default.json");
            Console.WriteLine(playlists);
        }



        
    }

}
