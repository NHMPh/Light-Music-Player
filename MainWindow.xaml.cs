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

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        WaveOutEvent output = new WaveOutEvent();
        MediaFoundationReader _mf;
        ThreadStart start;
        Thread thread;
        bool isChosingTimeStap;
        double timeInSong;
        bool isLoop;
        bool isAutoPlay;
        string currenturl =string.Empty;
        YoutubeDL ytdl = new YoutubeDL();

        Queue<VideoInfo> videoQueue = new Queue<VideoInfo>();
        Queue<VideoInfo> videoAutoQueue = new Queue<VideoInfo>();
        Queue<string> videoPlayListQueue2 = new Queue<string>();
        Queue<string> videoPlayListQueue = new Queue<string>();

        // CancellationTokenSource cts = new CancellationTokenSource();
        public MainWindow()
        {

            InitializeComponent();
            this.MouseDown += Window_MouseDown;
            pauseBtn.Style = null;
            Topmost = true;
            Icon = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\icon.ico")) ;
            start = new ThreadStart(TrackManager);
            thread = new Thread(start);
         
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();

            //  cts.Cancel();
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
                        if (isLoop)
                        {
                            SetTrackVisual();
                            _mf.Seek(0, SeekOrigin.Begin);
                           
                        }
                        else
                        {
                            if (videoQueue.Count != 0)
                            {
                                _mf = null;
                                songProgress.Value = 0;
                                Console.WriteLine("VideoQueue");
                                PlayMusic(videoQueue.Dequeue());
                               
                            }
                            else
                            {
                                if ( videoAutoQueue.Count != 0)
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
                        if(_mf != null)
                        songProgress.Value = _mf.CurrentTime.TotalMilliseconds;
                    }
                   
                });
                Thread.Sleep(1);
            }
        }
        private async void CheckQueue()
        {


            if (videoPlayListQueue2.Count > 0)
            {
                while (videoQueue.Count < 1)
                {
                    Console.WriteLine("run");

                    videoQueue.Enqueue(await Search(videoPlayListQueue2.Dequeue()));

                }
            }

            if (isAutoPlay)
            {

                if (videoPlayListQueue.Count == 0)
                {
                    GetPlayList();

                }
                while (videoAutoQueue.Count < 1)
                {

                    videoAutoQueue.Enqueue(await Search(videoPlayListQueue.Dequeue()));

                }

            }
            CheckNextSong();
        }
        private void songProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isChosingTimeStap)
            {
                thumb.Value = songProgress.Value;
            }

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
                    foreach (var item in videos)
                    {
                        videoPlayListQueue2.Enqueue(item.Url.ToString());
                    }
                    videoPlayListQueue2.Dequeue();
                    videoInfo.title = videos.First().Title;
                    videoInfo.description = $"Play lists with {videos.Count} videos";
                    videoInfo.thumbnail = videos.First().Thumbnails.FirstOrDefault().Url;
                    videoInfo.url = videos.First().Url;
                    return videoInfo;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error! Playlist link is wrong or not found");
                    return videoInfo;
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
     
            if (videoQueue.Count > 0)
            {
                PlayMusic(videoQueue.Dequeue());
               
            }
            if (videoQueue.Count == 0 && videoAutoQueue.Count != 0)
            {
                PlayMusic(videoAutoQueue.Dequeue());
            }
        }
        
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Add song to queue
            if (e.Key == Key.Enter)
            {
               
                    string key = searchBar.Text;
                searchBar.Text = null;
                videoQueue.Enqueue(await Search(key));

                if (output.PlaybackState == PlaybackState.Stopped)
                    PlayMusic(videoQueue.Dequeue());
            
            }
            CheckNextSong();
            if (e.Key == Key.T)
            {
                Console.WriteLine($"{videoQueue.Count} {videoAutoQueue.Count} {videoPlayListQueue.Count} {videoPlayListQueue2.Count}");
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
        private void volum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            output.Volume = (float)volum.Value;
            volumVisual.Value = volum.Value;
        }

        private void loopBtn_Click(object sender, RoutedEventArgs e)
        {
            isLoop = !isLoop;
            loopTxt.Text = isLoop ? " on " : " off ";
            Console.WriteLine(isLoop);
        }

        private void autoplay_btn_Click(object sender, RoutedEventArgs e)
        {
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
                videoPlayListQueue.Clear();
                videoAutoQueue.Clear();
                CheckNextSong();
            }
           

        }
        private async void GetPlayList()
        {
           
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

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(res => res.Content(content));
            var wcEndpointHrefs = document.QuerySelectorAll("a#wc-endpoint")
           .Select(a => a.GetAttribute("href"))
           .ToList();

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
            //remove the original
            videoPlayListQueue.Dequeue();
            CheckQueue();
            CheckNextSong();
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
        void CheckNextSong()
        {
            queue_txt.Text = $" {videoPlayListQueue2.Count + videoQueue.Count} ";
            next_txt.Text = videoQueue.Count != 0 ? $" {videoQueue.ElementAt(0).title}" : videoAutoQueue.Count != 0 ? $" {videoAutoQueue.ElementAt(0).title}" : $" None";
        }
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

        private async void Prevous_Btn(object sender, RoutedEventArgs e)
        {
            if (videoQueue.Count > 0)
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
            }

        }
    }

}
