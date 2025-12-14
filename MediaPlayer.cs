using Accord.Math.Distances;
using AngleSharp.Media;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeSearchApi.Net.Models.Youtube;
using static System.Net.Mime.MediaTypeNames;

namespace NHMPh_music_player
{
    public class MediaPlayer
    {

        private WaveOutEvent output = new WaveOutEvent();
        private _MediaFoundationReader _mf;
        private WaveChannel32 wave;
        FFTSampleProvider fftProvider;
        private MainWindow mainWindow;
        private VideoInfo currentSong;
        public event EventHandler OnSongChange;
        public event EventHandler OnPositionChange;
        private SpectrumVisualizer visualizer;
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        YoutubeDL ytdl;
        public VideoInfo CurrentSong { get { return currentSong; } set { currentSong = value; } }
        public WaveChannel32 Wave { get { return wave; } set { wave = value; } }
        public PlaybackState PlaybackState { get { return output.PlaybackState; } }

        public string PlayBackUrl;
        public float Volume { get { return output.Volume; } set { output.Volume = (float)value; } }
        public MediaPlayer(MainWindow mainWindow, SpectrumVisualizer visualizer)
        {
             ytdl = new YoutubeDL();
            this.mainWindow = mainWindow;
            this.visualizer = visualizer;
        }
        public async Task<string> GetStreamingUrl(string surl)
        {
            string url = "";
            try
            {
                mainWindow.youtube = new YoutubeClient();
                var streamManifest = await mainWindow.youtube.Videos.Streams.GetManifestAsync(surl);
                var streamUrl = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                url = streamUrl.Url;
                mainWindow.RefreshYoutubeClientHttpClient();

            }
            catch (Exception EX)
            {
                Console.WriteLine(EX.ToString());

                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.status.Text = "YTDL...";
                });
                try
                {
                    url = await GetRr5UrlAsync(surl);
                }
                catch (Exception ex3)
                {
                    await mainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        mainWindow.status.Text = "403";
                    });
                }

            }
            return url;

        }
        public async Task<string> GetRr5UrlAsync(string videoUrl)
        {
            
            var options = new OptionSet() { Format = "bestaudio", GetUrl = true,  ForceIPv4 = true };
           // options.AddCustomOption("--cookies", "cookie.txt");

            // cancel if yt-dlp hangs > 30s
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            string streamUrl ="";
            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                mainWindow.status.Text = "Reconnecting...";
            });
            try
            {
                options.Verbose = true;
            
                var result = await ytdl.RunWithOptions(
               new[] { $"https://www.youtube.com/watch?v={StringUtilitiy.ExtractId(videoUrl)}"},
               options,
               cts.Token
                 );

                // 🔹 Log what yt-dlp said
                Console.WriteLine("--- STDOUT ---");
                Console.WriteLine(result.Data); // normal log
                Console.WriteLine("--- STDERR ---");
                foreach(var error in result.ErrorOutput)
                {
                    foreach(var er in error)
                    {
                        Console.Write(er.ToString());
                        
                    }
                    Console.WriteLine();
                }

                // 🔹 Check if it actually succeeded
                if (result.Success && result.Data != null && result.Data.Length > 0)
                {
                     streamUrl = result.Data[0];
                    Console.WriteLine("✅ Got stream URL: " + streamUrl);
                    return streamUrl;
                }
                else
                {
                    Console.WriteLine("⚠️ yt-dlp failed:\n" + result.ErrorOutput);
                    return null;
                }
               
            }
            catch(Exception ex)
            {
                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.status.Text = "Failed";
                });
                MessageBox.Show("Failed to get stream url via YTDL.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await Task.Delay(1000);
              
               //return await GetRr5UrlAsync(videoUrl);
            }

            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                mainWindow.status.Text = "Done...";
            });
            Console.WriteLine(streamUrl);
            return streamUrl;
        }
        private async Task GetSongStream()
        {

            string url = await GetStreamingUrl(currentSong.Url);

            Console.WriteLine(currentSong.Url);
            


            Console.WriteLine(url);
           
           
            _mf = new _MediaFoundationReader(url);
            Console.WriteLine(_mf.WaveFormat.SampleRate);
            if (_mf.WaveFormat.SampleRate == 44100) { 
               YoutubeVideo newVid = await Searcher.ReSearch($"{currentSong.Title}+{currentSong.Artist}");

                string newUrl = await GetStreamingUrl(newVid.Url);
                Console.WriteLine("Re-Searched URL: " + newUrl);
                _mf = new _MediaFoundationReader(newUrl);
            }
            
           
            //  PlayBackUrl = streamUrl;
            wave = new NAudio.Wave.WaveChannel32(_mf);
            Console.WriteLine("5");
            fftProvider = new FFTSampleProvider(wave.ToSampleProvider(), visualizer);
            Console.WriteLine("6");


        }
        private void GetRadioStream()
        {
            _mf = new _MediaFoundationReader(currentSong.Url);
            wave = new WaveChannel32(_mf);
            fftProvider = new FFTSampleProvider(wave.ToSampleProvider(), visualizer);
        }

        public MediaPlayer(VideoInfo videoInfo)
        {

            currentSong = videoInfo;
        }

        public async Task PlayRadio()
        {
            MusicSetting.isRadio = true;

            mainWindow.status.Text = "Please wait";


            await Task.Run(() => GetRadioStream());

            OnSongChange?.Invoke(this, null);
            output?.Dispose();
            output.Init(fftProvider);
            output.Play();
            mainWindow.status.Text = "Playing";

        }
        public async void PlayMusic()
        {
            MusicSetting.isRadio = false;
            mainWindow.status.Text = "Loading...";
            await Task.Run(() => GetSongStream());

            OnSongChange?.Invoke(this, null);

            Console.WriteLine("Playing");

            output?.Dispose();
            //output = new WaveOut();
            output.Init(fftProvider);

            output.Play();
            mainWindow.status.Text = "Playing";


        }
        public void PauseMusic()
        {
            output.Pause();
            mainWindow.status.Text = "Paused";
            mainWindow.playpause.Source = new BitmapImage(
            new Uri($"{Environment.CurrentDirectory}\\Images\\_Play.png"));
        }
        public void _PauseMusic()
        {
            output.Pause();
        }
        public void ResumeMusic()
        {
            output.Play();
            mainWindow.status.Text = "Playing";
            mainWindow.playpause.Source = new BitmapImage(
            new Uri($"{Environment.CurrentDirectory}\\Images\\_Pause.png"));
        }
        public void Seek(int seconds)
        {
            Console.WriteLine("Stop");

            wave.Position = wave.WaveFormat.AverageBytesPerSecond * seconds;
            if (output.PlaybackState != PlaybackState.Playing)
                output.Play();

            mainWindow.songProgress.Value = wave.CurrentTime.TotalMilliseconds;
            OnPositionChange?.Invoke(this, null);
        }

        public int GetMasterPeak()
        {
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            return (int)(60 * device.AudioMeterInformation.MasterPeakValue);
        }
    }
}
