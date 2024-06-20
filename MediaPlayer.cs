using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VideoLibrary;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using static System.Windows.Forms.LinkLabel;

namespace NHMPh_music_player
{
    public class MediaPlayer
    {
        private DispatcherTimer timer;

        private WaveOutEvent output = new WaveOutEvent();
        private MediaFoundationReader _mf;
        private WaveChannel32 wave;
        FFTSampleProvider fftProvider;
        private MainWindow mainWindow;
        private VideoInfo currentSong;
        public event EventHandler OnSongChange;
        public event EventHandler OnPositionChange;
        private SpectrumVisualizer visualizer;
        public VideoInfo CurrentSong { get { return currentSong; } set { currentSong = value; } }
        public WaveChannel32 Wave { get { return wave; } set { wave = value; } }
        public PlaybackState PlaybackState { get { return output.PlaybackState; } }

        public string PlayBackUrl;
        public float Volume { set { output.Volume = (float)value; } }
        public MediaPlayer(MainWindow mainWindow, SpectrumVisualizer visualizer)
        {
            this.mainWindow = mainWindow;
            this.visualizer = visualizer;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(2000); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await Play();
            };

        }

        private async Task Play()
        {
            output.Play();
            timer.Stop();
            //  mainWindow.status.Text = "Playing";
        }

        private async Task GetSongStream()
        {


            // var ytdl = new YoutubeDL();
            //  OptionSet options = new OptionSet() { Format = "mp4", GetUrl = true };





            var streamManifest = await mainWindow.youtube.Videos.Streams.GetManifestAsync(currentSong.Url);

            var streamUrl = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality().Url;


            /*  RunResult<string[]> streamUrl;
              streamUrl = await ytdl.RunWithOptions(
                 new[] { currentSong.Url },
                 options,
                 CancellationToken.None
             );*/
            // description.Text = streamUrl.Data[0];
            PlayBackUrl = streamUrl;
            Console.WriteLine(streamUrl);
            //set _mf for playing audio


            _mf = new MediaFoundationReader(streamUrl);
            ;
            wave = new WaveChannel32(_mf);
            fftProvider = new FFTSampleProvider(wave.ToSampleProvider(), visualizer);
        }
        private void GetRadioStream()
        {
            _mf = new MediaFoundationReader(currentSong.Url);
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
            //   timer.Start();


        }
        public async void PlayMusic()
        {
            MusicSetting.isRadio = false;
            mainWindow.status.Text = "Loading...";
            await GetSongStream();
            OnSongChange?.Invoke(this, null);
            output?.Dispose();
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
            wave.Position = wave.WaveFormat.AverageBytesPerSecond * seconds;
            if (output.PlaybackState != PlaybackState.Playing)
                output.Play();

            mainWindow.songProgress.Value = wave.CurrentTime.TotalMilliseconds;
            OnPositionChange?.Invoke(this, null);
        }
    }
}
