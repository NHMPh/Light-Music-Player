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
using static System.Net.Mime.MediaTypeNames;
using AngleSharp.Media;
using System.ComponentModel;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using Accord.Math;
using System.Numerics;
using System.Windows.Threading;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isChromeOpen = false;
        bool isDragging = false;
        bool isLyrics = false;
        WaveOutEvent output = new WaveOutEvent();
        MediaFoundationReader _mf;
        WaveChannel32 wave;
        MediaFoundationReader _mfSpectrum;
        WaveChannel32 waveSpectrum;
        bool isChosingTimeStap;
        double timeInSong;
        bool isLoop;
        bool isAutoPlay;
        bool isSpectrum;
        public static double[] fbands = new double[2048];
        float[] decreaserate = new float[512];
        VideoInfo currenturl = new VideoInfo();
        YoutubeDL ytdl = new YoutubeDL();
        public static string currentCustomPlayList;
        //Normal video info
        Queue<VideoInfo> videoQueue = new Queue<VideoInfo>();
        //Normal video info for autoplay
        Queue<VideoInfo> videoAutoQueue = new Queue<VideoInfo>();
        List<CustomPlaylist> activeWindow = new List<CustomPlaylist>();
        OptionSet options = new OptionSet() { Format = "m4a", GetUrl = true };
        RunResult<string[]> streamUrl;
        YoutubeClient youtube = new YoutubeClient();
        static HttpClient httpClient = new HttpClient();
        YoutubeSearchClient client = new YoutubeSearchClient(httpClient);
        static MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        List<ProgressBar> spectrumBars = new List<ProgressBar>();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        FullscreenSpectrum fullscreenSpectrum = new FullscreenSpectrum();
        JArray songLyrics = new JArray();
        long first = 0;
        private DispatcherTimer timer;
        public MainWindow()
        {

            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await TrackManager();
            };
            CreateSpectrumBar();
            WarmUp();
            LoadCustomPlayList();
            this.MouseDown += Window_MouseDown;
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            pauseBtn.Style = null;
            Topmost = true;
            Icon = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\icon.ico"));
            // start = new ThreadStart(TrackManager);
            // thread = new Thread(start);
            Closed += MainWindow_Closed;
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void UpdateGraph()
        {
            while (waveSpectrum.Position <= wave.Position)
            {
                fbands = GetFFTdata();
            }
        }
        private void DrawGraph()
        {

            for (int i = 0, j = 0; i < 126; i++)
            {
                int mutipler = 5000;
                if (i < 50) mutipler = 2000;


                if (fbands[i] * mutipler > spectrumBars[i].Value)
                {


                    spectrumBars[i].Value = ((fbands[i + j] + fbands[i + j + 1]) / 2) * mutipler;
                    j++;



                    decreaserate[i] = 10f;
                }
                else
                {
                    spectrumBars[i].Value -= 1 * decreaserate[i];
                    decreaserate[i] *= 1.3f;
                }
            }

        }
        private double[] GetFFTdata()
        {
            int desireByte = (int)Math.Pow(2, 13);
            double[] doublesData = new double[desireByte / 4];
            doublesData.Clear();
            byte[] buffer = new byte[desireByte];
            int bytesRead = 0;
            try
            {
                bytesRead = waveSpectrum.Read(buffer, 0, desireByte);

            }
            catch
            {
                output.Play();
                Console.WriteLine("Cant read");
                return null;
            }
            for (int i = 0; i < bytesRead / 4; i++)
            {
                doublesData[i] = BitConverter.ToSingle(buffer, i * 4) * 10;
            }
            return FFT(doublesData);

        }
        public double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            Complex[] fftComplex = new Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                fftComplex[i] = new Complex(data[i], 0.0);
            }
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            for (int i = 0; i < data.Length; i++)
            {
                fft[i] = fftComplex[i].Magnitude;

            }
            return fft;
        }
        private void CreateSpectrumBar()
        {
            for (int i = 0; i < 126; i++)
            {

                ProgressBar progressBar = new ProgressBar()
                {
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = new SolidColorBrush(Colors.Cyan),
                    Width = 2,
                    Margin = new Thickness(0, 0, 1, 0),
                    Height = 65,
                    Maximum = 1000,
                    Orientation = Orientation.Vertical,
                    Value = 0
                };

                //  spectrum_ctn.Children.Add(progressBar);
                spectrumBars.Add(progressBar);
            }

            for (int i = 0; i < 126; i++)
            {
                if (i < 63)
                {
                    spectrum_ctn.Children.Add(spectrumBars[(62 - i) * 2]);

                }
                else
                {
                    spectrum_ctn.Children.Add(spectrumBars[Math.Abs((62 - i) * 2 + 1)]);

                }

            }

        }

        private async void WarmUp()
        {
            var url = await Search("Rick roll");
            streamUrl = await ytdl.RunWithOptions(
                new[] { url.url },
                options,
                CancellationToken.None
            );
            _mf = new MediaFoundationReader(streamUrl.Data[0]);

            //play audio
            if (output != null)
                output.Dispose();
            output.Init(_mf);
            output.Play();

            if (output.PlaybackState == PlaybackState.Playing)
            {
                output.Stop();
                output.Dispose();
                status.Text = "Status: Hello";
                (startup.Parent as Grid).Children.Remove(startup);
            }

        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                window.Close();
            }
        }

        private void LoadCustomPlayList(object sender, EventArgs e)
        {



            comboboxCustomPlayList.SelectionChanged -= comboboxCustomPlayList_SelectionChanged;
            comboboxCustomPlayList.Items.Clear();

            // Get all file names in the folder
            string[] fileNames = Directory.GetFiles(".\\custom\\");

            if (fileNames.Length != 0)
            {
                if (currentCustomPlayList == null)
                {
                    currentCustomPlayList = Path.GetFileNameWithoutExtension(fileNames[0]);
                }
                activeWindow.Remove(sender as CustomPlaylist);
                CustomPlaylist temp = sender as CustomPlaylist;
                temp.Uid = currentCustomPlayList;
                activeWindow.Add(temp);
                customPlname.Text = currentCustomPlayList;
                // Print all file names
                foreach (string fileName in fileNames)
                {

                    ComboBoxItem comboBoxItem = new ComboBoxItem()
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),
                        Content = Path.GetFileNameWithoutExtension(fileName)
                    };
                    comboboxCustomPlayList.Items.Add(comboBoxItem);
                }

            }
            else
            {
                customPlname.Text = "Click here";
            }

            ComboBoxItem _comboBoxItem = new ComboBoxItem()
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                BorderThickness = new Thickness(0),
                Content = "CREATE NEW PLAYLIST"
            };
            comboboxCustomPlayList.Items.Add(_comboBoxItem);
            comboboxCustomPlayList.SelectionChanged += comboboxCustomPlayList_SelectionChanged;
        }

        private async void PlayMusic(VideoInfo videoInfo)
        {
            status.Text = "Loading (0%)...";

            if (videoInfo.url == null) return;
            currenturl = videoInfo;
            isLyrics = false;
            //Convert url to audiable link
            streamUrl = await ytdl.RunWithOptions(
                new[] { videoInfo.url },
                options,
                CancellationToken.None
            );
            Console.WriteLine(streamUrl.Data[0]);
            status.Text = "Loading (25%)...";


            _mf = new MediaFoundationReader(streamUrl.Data[0]);
            status.Text = "Loading (50%)...";
            wave = new WaveChannel32(_mf);
            status.Text = "Loading (75%)...";
            _mfSpectrum = new MediaFoundationReader(streamUrl.Data[0]);
            waveSpectrum = new WaveChannel32(_mfSpectrum);
            //play audio
            if (output != null)
                output.Dispose();
            output.Init(wave);
            output.Play();

            status.Text = "Loading (75%)...";
            FindLyrics(videoInfo.title);

            //Start track thread
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
            SetTrackVisual();
            CheckQueue();
            CheckNextSong();
            status.Text = "Loaded";

            currenturl.title = videoInfo.title;
            currenturl.description = videoInfo.description;
            currenturl.url = videoInfo.url;
            SetVisual("" + videoInfo.title, videoInfo.description, videoInfo.thumbnail);
            currenturl.thumbnail = videoInfo.thumbnail;
            var des = await ytdl.RunVideoDataFetch(videoInfo.url);
            currenturl.description = videoInfo.description + "\n" + des.Data.Description;
            SetVisualDes(currenturl.description);

        }

        private async void FindLyrics(string name)
        {
            name = ProcessInvailName(name);
            Console.WriteLine($"https://api.textyl.co/api/lyrics?q={name}");
            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send GET request to a URL
                    HttpResponseMessage response = await client.GetAsync($"https://api.textyl.co/api/lyrics?q={name}");

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the content as string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        songLyrics = JArray.Parse(responseBody);
                        Console.WriteLine(songLyrics);
                        desGrid.Children.Add(new Button()
                        {
                            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x28, 0x2B, 0x30)),
                            Margin = new Thickness(0, 0, 0, 0),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xA6, 0xB3)),
                            Padding = new Thickness(0),
                            BorderThickness = new Thickness(0),
                            Width = 90,
                            Content = "-Lyric availible",
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                            FontSize = 12,
                            VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        });

                        // Apply the style for the border
                        Style borderStyle = new Style(typeof(Border));
                        borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(3)));
                        ((Button)desGrid.Children[desGrid.Children.Count - 1]).Resources.Add(typeof(Border), borderStyle);
                        // Attach click event handler
                        ((Button)desGrid.Children[desGrid.Children.Count - 1]).Click += enable_lyric;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
        private void enable_lyric(object sender, RoutedEventArgs e)
        {
            isLyrics = true;
            description.Text = "For more accurate lyrics sync, search \"[song name] + lyrics.\"";
            if (sender is Button clickedButton)
            {
                desGrid.Children.Remove(clickedButton);
            }
        }
        private string ProcessInvailName(string name)
        {
            if (name.Contains('‒'))
                name = name.Replace('‒', '-');

            string result = name;
            if (!result.Contains("-")) return result.Replace(" ", "%20");
            string[] parts = Regex.Split(result, @"(?<=\s-\s)|(?<=\s--\s)|(?<=-\s)");
            parts[0] = Regex.Replace(parts[0], @"(\([^)]*\)|\[[^\]]*\])|ft\..*|FT\..*|Ft\..*|feat\..*|Feat\..*|FEAT\..*|【|】", "");
            parts[1] = Regex.Replace(parts[1], @"(\([^)]*\)|\[[^\]]*\])|ft\..*|FT\..*|Ft\..*|feat\..*|Feat\..*|FEAT\..*|【|】", "");
            if (parts[0].Contains('&'))
            {
                parts[0] = parts[0].Split('&')[parts[0].Split('&').Length - 1];
            }
            parts[1] = ReplaceNumbersWithWords(parts[1]);
            string processName = parts[0] + " " + parts[1];
            return processName.Replace(" ", "%20");

        }
        private static readonly Dictionary<int, string> numberWords = new Dictionary<int, string>()
        {
        {1, "one"}, {2, "two"}, {3, "three"}, {4, "four"}, {5, "five"},
        {6, "six"}, {7, "seven"}, {8, "eight"}, {9, "nine"}, {10, "ten"}
        };
        public static string NumberToWords(int number)
        {
            if (number < 1 || number > 10)
            {
                throw new ArgumentOutOfRangeException("Number must be between 1 and 10.");
            }

            return numberWords[number];
        }
        public static string ReplaceNumbersWithWords(string input)
        {
            string[] parts = input.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                // Attempt to parse the substring as an integer
                if (int.TryParse(parts[i], out int num))
                {
                    // If the parsed number is less than 10, replace it with its string representation
                    if (num < 10)
                    {
                        for (int j = 1; j <= 10; j++)
                        {
                            parts[i] = num.ToString().Replace(j.ToString(), NumberToWords(j));
                        }
                    }

                }
            }

            input = string.Join(" ", parts);
            return input;
        }
        private void SetVisualDes(string des)
        {
            if (!isLyrics)
                description.Text = des;
        }
        private void SetTrackVisual()
        {
            songProgress.Value = 0;
            thumb.Value = 0;
            songProgress.Maximum = wave.TotalTime.TotalMilliseconds;
            thumb.Maximum = wave.TotalTime.TotalMilliseconds;
            //songProgress.Maximum = wave.TotalTime.TotalSeconds;
            //thumb.Maximum = wave.TotalTime.TotalSeconds;
        }
        private async Task TrackManager()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    // Console.WriteLine(_mf.Position);
                   
                        if (!isDragging && output.PlaybackState == PlaybackState.Playing && isSpectrum)
                        {
                            UpdateGraph();
                            DrawGraph();
                        }
                   

                    //Song end
                    if (songProgress.Value == songProgress.Maximum)
                    {
                        //When loop is on
                        if (isLoop)
                        {
                            SetTrackVisual();
                            wave.Seek(0, SeekOrigin.Begin);
                            if (output.PlaybackState != PlaybackState.Playing)
                            {
                                output.Play();
                            }
                            waveSpectrum.Seek(0, SeekOrigin.Begin);
                        }

                        else
                        {
                            //When there's song(s) in queue
                            if (videoQueue.Count != 0)
                            {
                                _mf = null;
                                songProgress.Value = 0;
                                PlayMusic(videoQueue.Dequeue());

                            }
                            //No song in queue but autoplay is on
                            else
                            {
                                if (videoAutoQueue.Count != 0)
                                {
                                    _mf = null;
                                    songProgress.Value = 0;
                                    PlayMusic(videoAutoQueue.Dequeue());

                                    songProgress.Value = 0;

                                }
                            }
                        }

                    }
                    else
                    {
                        if (_mf != null)
                            songProgress.Value = wave.CurrentTime.TotalMilliseconds;
                        if (isLyrics)
                        {

                            foreach (var lyric in songLyrics)
                            {
                                if (lyric["seconds"].ToString() == ((int)wave.CurrentTime.TotalSeconds).ToString())
                                {

                                    description.Text = lyric["lyrics"].ToString();
                                }
                            }

                        }
                    }
                });
            });

        }
        private async void CheckQueue()
        {

            if (videoAutoQueue.Count == 0)
            {
                if (isAutoPlay)
                {
                    GetPlayList();
                }
            }


            CheckNextSong();
        }
        private async Task<VideoInfo> Search(string key)
        {
            status.Text = "Searching...";
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
                    status.Text = "...";
                    return videoInfo;
                }
                catch
                {
                    MessageBox.Show("Error! This youtube link is wrong or not found");
                    status.Text = "...";
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
                    status.Text = "...";
                    return videoInfo;
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error! Auto-generated playlist link is not supported or playlist is private");
                    status.Text = "...";
                    return null;


                }


            }

            //Search by key

            client = new YoutubeSearchClient(httpClient);
            var responseObject = await client.SearchAsync(key);
            foreach (YoutubeVideo video in responseObject.Results)
            {
                videoInfo.description = $"{video.Author} - {video.Duration}";
                videoInfo.title = video.Title;
                videoInfo.url = video.Url;
                videoInfo.thumbnail = video.ThumbnailUrl;
                status.Text = "...";
                return videoInfo;
            }
            status.Text = "...";
            return null;


        }
        private void SetVisual(string _title, string _desciption, string _thumbnail)
        {
            title.Text = _title;
            description.Text = _desciption;
            thumbnail.ImageSource = new BitmapImage(
             new Uri(_thumbnail));
        }
        private async void GetPlayList()
        {
            isChromeOpen = true;
            status.Text = "Fetching";
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
                try
                {
                    browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        ExecutablePath = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe"
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

            }
            string videoUrl = $"{currenturl}&list=RD{ExtractId(currenturl.url)}&themeRefresh=1";
            var page = await browser.NewPageAsync();
            await page.GoToAsync(videoUrl);
            //style-scope ytd-playlist-panel-renderer
            await page.WaitForSelectorAsync(".style-scope.ytd-playlist-panel-renderer");
            await page.WaitForSelectorAsync("a#wc-endpoint");
            await page.WaitForSelectorAsync("img");
            await page.WaitForNetworkIdleAsync();
            var content = await page.GetContentAsync();
            await browser.CloseAsync();
            isChromeOpen = false;
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(res => res.Content(content));
            var wcEndpointHrefs = document.QuerySelectorAll("a#wc-endpoint")
           .Select(a => a.GetAttribute("href"))
           .ToList();
            var wcEndpointTitles = document.QuerySelectorAll("#video-title")
          .Select(a => a.GetAttribute("title"))
          .ToList();
            videoAutoQueue.Clear();

            List<string> urls = new List<string>();
            List<string> titles = new List<string>();
            List<string> thumbnails = new List<string>();

            int stopCount = 2;
            // Print the extracted href values
            foreach (var href in wcEndpointHrefs)
            {
                if (stopCount == 2)
                {
                    urls.Add($"https://www.youtube.com/watch?v={ExtractId(href)}");
                    thumbnails.Add($"https://i.ytimg.com/vi/{ExtractId(href)}/hqdefault.jpg?sqp=-oaymwEjCOADEI4CSFryq4qpAxUIARUAAAAAGAElAADIQj0AgKJDeAE=&rs=AOn4CLBzQ9ogPrePWJD7x2FhmZKlos8bDA");
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
                    titles.Add(title);
                    stopCount++;
                }
            }
            for (int i = 0; i < 25; i++)
            {
                videoAutoQueue.Enqueue(new VideoInfo()
                {
                    title = titles[i],
                    thumbnail = thumbnails[i],
                    url = urls[i],
                    description = "From autoplay",
                });
                Console.WriteLine($"Thumbnail: {thumbnails[i]} Title: {titles[i]} Url: {urls[i]} ");
            }
            status.Text = "Playlist loaded";
            //remove the original
            videoAutoQueue.Dequeue();
            CheckQueue();

        }
        //Call when song change
        void CheckNextSong()
        {
            combobox.SelectionChanged -= Combobox_SelectionChanged;
            combobox.Items.Clear();
            if (videoQueue.Count + videoAutoQueue.Count > 0)
            {
                //Add videoQueue 1st
                int j = 0;
                for (int i = 0; i < videoQueue.Count; i++)
                {
                    StackPanel stackPanel = CreatePlaylistSelection(videoQueue.ElementAt(i).thumbnail, videoQueue.ElementAt(i).title, j);
                    combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = stackPanel,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00F9FF")),
                        BorderThickness = new Thickness(0),

                    });
                    j++;
                }

                for (int i = 0; i < videoAutoQueue.Count; i++)
                {

                    StackPanel stackPanel = CreatePlaylistSelection(videoAutoQueue.ElementAt(i).thumbnail, videoAutoQueue.ElementAt(i).title, j);
                    combobox.Items.Add(new ComboBoxItem()
                    {
                        Content = stackPanel,
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
            queue_txt.Text = $" {videoQueue.Count + videoAutoQueue.Count} ";
            next_txt.Text = videoQueue.Count != 0 ? $"{videoQueue.ElementAt(0).title}" : videoAutoQueue.Count != 0 ? $"{videoAutoQueue.ElementAt(0).title}" : $"Click to select song from queue";
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
                Text = id + ". " + title
            };
            Button button = new Button();
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF282B30"));
            button.BorderThickness = new Thickness(0);
            button.Width = 50;
            button.Margin = new Thickness(7, 0, 0, 0);
            button.Content = "Remove";
            button.Foreground = Brushes.White;
            button.FontSize = 12;
            button.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            button.Height = 20;
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
            if (id < videoQueue.Count)
            {

                RemoveAtIndex(videoQueue, id);
            }
            else
            {
                RemoveAtIndex(videoAutoQueue, id - videoQueue.Count);
            }
            CheckNextSong();
        }

        private async void MoveTrackManager(string track)
        {

            int id = ExtractTrackId(track);
            if (id < videoQueue.Count)
            {
                var videoTemp = videoQueue.ElementAt(id);
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                videoQueue.Enqueue(videoTemp);
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                RemoveAtIndex(videoQueue, id + 1);
            }
            else
            {
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
                videoQueue.Enqueue(videoAutoQueue.ElementAt(id - videoQueue.Count));
                RemoveAtIndex(videoAutoQueue, id - videoQueue.Count + 1);
                videoQueue = new Queue<VideoInfo>(videoQueue.Reverse());
            }
            CheckNextSong();
        }



        //Support function
        public static int EvaluateKeyWord(string key)
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
                Console.WriteLine("search");
                return 3;
            }
        }
        public static bool IsYouTubeVideoLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/watch\?v=|youtu\.be/)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        public static bool IsYouTubePlaylistLink(string input)
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
            Console.WriteLine("Overlay");
            int second = (int)Math.Floor(thumb.Value / 1000);
            int current = (int)Math.Floor(timeInSong / 1000);
            int secondToSkip = second - current;
            Console.WriteLine($"{secondToSkip}");


            wave.Skip(secondToSkip);


            songProgress.Value = wave.CurrentTime.Seconds * 1000;
            isChosingTimeStap = false;
            if (output.PlaybackState != PlaybackState.Playing)
            {
                output.Play();
            }

            waveSpectrum.Position = wave.Position;
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

            wave.Skip(secondToSkip);
            songProgress.Value = wave.CurrentTime.Seconds * 1000;

            isChosingTimeStap = false;
            if (output.PlaybackState != PlaybackState.Playing)
            {
                output.Play();
            }
            waveSpectrum.Position = wave.Position;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (currenturl.url == string.Empty) return;
            output.Pause();



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
            status.Text = "Skipping...";
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
            if (currenturl.url == string.Empty)
            {
                MessageBox.Show("Play any song to use autoplay");
                return;
            }
            isAutoPlay = !isAutoPlay;

            if (isAutoPlay)
            {
                auto_txt.Text = " on ";
                if (videoAutoQueue.Count == 0)
                {
                    GetPlayList();
                }
            }
            else
            {
                auto_txt.Text = " off ";
                //remove later

                videoAutoQueue.Clear();

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

                CheckNextSong();
            }
            if (e.Key == Key.T)
            {
                Console.WriteLine($"{videoQueue.Count} {videoAutoQueue.Count}");
                Console.WriteLine(_mf.Length);
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                isDragging = true;
            this.DragMove();


        }


        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as StackPanel;
            if (text != null)
                MoveTrackManager((text.Children[1] as TextBlock).Text);
        }
        private void comboboxCustomPlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            if (text == "CREATE NEW PLAYLIST")
            {

                string[] fileNames = Directory.GetFiles(".\\custom\\");
                int count = 0;
                foreach (string fileName in fileNames)
                {

                    if (Path.GetFileNameWithoutExtension(fileName).Contains("New playlist")) count++;
                }
                var data = new JObject(
                new JProperty("thumbnail", "https://i.ytimg.com/vi/J3pF2jkQ4vc/hq720.jpg?sqp=-oaymwEcCOgCEMoBSFXyq4qpAw4IARUAAIhCGAFwAcABBg==&rs=AOn4CLB2BGz1gQ9O8LD0Y4NcWdEfaYgAyw"),
                 new JProperty("title", "New playlist"),
                 new JProperty("songs", new JArray())
                                      );
                if (count == 0)
                {
                    System.IO.File.WriteAllText(".\\custom\\New playlist.json", data.ToString());

                    customPlname.Text = "New playlist";
                    currentCustomPlayList = "New playlist";
                }
                else
                {
                    System.IO.File.WriteAllText($".\\custom\\New playlist ({count}).json", data.ToString());
                    customPlname.Text = $"New playlist ({count})";
                    currentCustomPlayList = $"New playlist ({count})";
                }
                OpenNewWindow();
                LoadCustomPlayList();
                return;

            }

            customPlname.Text = text;
            currentCustomPlayList = text;
        }
        //////////////
        private void LoadCustomPlayList()
        {
            comboboxCustomPlayList.SelectionChanged -= comboboxCustomPlayList_SelectionChanged;
            comboboxCustomPlayList.Items.Clear();

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
                        Content = Path.GetFileNameWithoutExtension(fileName)
                    };
                    comboboxCustomPlayList.Items.Add(comboBoxItem);
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
            comboboxCustomPlayList.Items.Add(_comboBoxItem);
            comboboxCustomPlayList.SelectionChanged += comboboxCustomPlayList_SelectionChanged;
        }
        //Custom playlist


        private void load_custom_btn(object sender, RoutedEventArgs e)
        {
            var playlists = ReadJsonFile($".\\custom\\{currentCustomPlayList}.json");
            if (playlists == null) return;
            for (int i = 0; i < playlists["songs"].Count(); i++)
            {
                videoQueue.Enqueue(new VideoInfo()
                {
                    title = playlists["songs"][i]["title"].ToString(),
                    thumbnail = playlists["songs"][i]["thumbnail"].ToString(),
                    url = playlists["songs"][i]["url"].ToString(),
                    description = "Song from your custom playlist"

                });
            }
            CheckNextSong();
            if (output.PlaybackState == PlaybackState.Stopped)
                PlayMusic(videoQueue.Dequeue());
            status.Text = "Playlist loaded";
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
        private void view_custom_btn(object sender, RoutedEventArgs e)
        {
            OpenNewWindow();
        }

        private void RemoveActiveWindow(object sender, CancelEventArgs e)
        {
            activeWindow.Remove(sender as CustomPlaylist);
        }

        private void close_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                window.Close();
            }
        }

        private void spectrum_btn_Click(object sender, RoutedEventArgs e)
        {
            isSpectrum = !isSpectrum;
            if (!isSpectrum)
            {
                foreach (var spec in spectrumBars)
                {
                    spec.Value = 0;
                }
            }
            if (isSpectrum)
            {

                description.Height = 16;
                spectrum_ctn.Height = 60;
                waveSpectrum.Position = wave.Position;
                UpdateGraph();
                DrawGraph();

            };

        }

        private void minimize_btn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void spectrum_btn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!fullscreenSpectrum.IsActive)
            {
                fullscreenSpectrum.Show();
            }
            else
            {
                fullscreenSpectrum.Focus();
            }
        }
    }

}
