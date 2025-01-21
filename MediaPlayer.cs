using Accord.Math.Distances;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
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
using YoutubeExplode.Videos.Streams;

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

        public VideoInfo CurrentSong { get { return currentSong; } set { currentSong = value; } }
        public WaveChannel32 Wave { get { return wave; } set { wave = value; } }
        public PlaybackState PlaybackState { get { return output.PlaybackState; } }

        public string PlayBackUrl;
        public float Volume { get { return output.Volume; } set { output.Volume = (float)value; } }
        public MediaPlayer(MainWindow mainWindow, SpectrumVisualizer visualizer)
        {

            this.mainWindow = mainWindow;
            this.visualizer = visualizer;
        }

        OptionSet options = new OptionSet() { Format = "m4a", GetUrl = true };
        private async Task GetSongStream()
        {

            string url;

            Console.WriteLine(currentSong.Url);
            try
            {
                var streamManifest = await mainWindow.youtube.Videos.Streams.GetManifestAsync(currentSong.Url);
                var streamUrl = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();//streamManifest.GetMuxedStreams().GetWithHighestVideoQuality().Url;
                System.IO.Stream stream = await mainWindow.youtube.Videos.Streams.GetAsync(streamUrl);
                Console.WriteLine(stream.Length);
                url = streamUrl.Url;
            }
            catch
            {
                if (!File.Exists(".\\yt-dlp.exe")) //if ytdl is not downloaded
                {
                    string message = "Downloading yt-dlp.exe please wait: https://github.com/yt-dlp/yt-dlp";
                    string caption = "Download yt-dlp";
                    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    await YoutubeDLSharp.Utils.DownloadYtDlp();
                }                  
                else
                {
                   
                }
                var _streamUrl = await mainWindow.ytdl.RunWithOptions(
                     new[] { currentSong.Url },
                     options,
                     CancellationToken.None
                );
                url = _streamUrl.Data[0];
            }




            _mf = new _MediaFoundationReader(url);
            //  PlayBackUrl = streamUrl;
            wave = new NAudio.Wave.WaveChannel32(_mf);
            fftProvider = new FFTSampleProvider(wave.ToSampleProvider(), visualizer);

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
