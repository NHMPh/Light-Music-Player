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

        private async Task GetSongStream()
        {

            string url="";

            Console.WriteLine(currentSong.Url);
            try
            {
                mainWindow.RefreshYoutubeClientHttpClient();
                var streamManifest = await mainWindow.youtube.Videos.Streams.GetManifestAsync(currentSong.Url);
                var streamUrl = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();//streamManifest.GetMuxedStreams().GetWithHighestVideoQuality().Url;
                System.IO.Stream stream = await mainWindow.youtube.Videos.Streams.GetAsync(streamUrl);
                Console.WriteLine(stream.Length);
                url = streamUrl.Url;
            }
            catch(Exception EX)
            {
               
                mainWindow.RefreshYoutubeClientHttpClient();
                try
                {
                    MessageBox.Show("Refreshed Youtube Client HttpClient. Retrying...");
                    var streamManifest = await mainWindow.youtube.Videos.Streams.GetManifestAsync(currentSong.Url);
                    var streamUrl = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();//streamManifest.GetMuxedStreams().GetWithHighestVideoQuality().Url;
                    System.IO.Stream stream = await mainWindow.youtube.Videos.Streams.GetAsync(streamUrl);
                    Console.WriteLine(stream.Length);
                    url = streamUrl.Url;
                }
                catch (Exception EX2)
                {
                    MessageBox.Show(EX2.ToString());
                }
                   
               
            }



            _mf = new  _MediaFoundationReader(url);
            Console.WriteLine("4");
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
