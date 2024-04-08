using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace NHMPh_music_player
{
    internal class MediaPlayer
    {
        private WaveOutEvent output = new WaveOutEvent();
        private MediaFoundationReader _mf;
        private WaveChannel32 wave;
        private MainWindow mainWindow;
        private VideoInfo currentSong;
        public event EventHandler OnSongChange;
        public event EventHandler OnPositionChange;
        public VideoInfo CurrentSong { get { return currentSong; } set { currentSong = value; } }
        public WaveChannel32 Wave { get { return wave; } set { wave = value; } }
        public PlaybackState PlaybackState { get { return output.PlaybackState; } }

        public string PlayBackUrl;
        public float Volume { set { output.Volume = (float)value; } }
        public MediaPlayer(MainWindow mainWindow)
        {        
            this.mainWindow = mainWindow;
        }
        private async Task GetSongStream()
        {
            var ytdl = new YoutubeDL();
            OptionSet options = new OptionSet() { Format = "mp4", GetUrl = true };
            RunResult<string[]> streamUrl;
            streamUrl = await ytdl.RunWithOptions(
               new[] { currentSong.Url },
               options,
               CancellationToken.None
           );
            // description.Text = streamUrl.Data[0];
            PlayBackUrl = streamUrl.Data[0];
            Console.WriteLine(streamUrl.Data[0]);
            //set _mf for playing audio
            _mf = new MediaFoundationReader(streamUrl.Data[0]);
            wave = new WaveChannel32(_mf);
        }
        public MediaPlayer(VideoInfo videoInfo)
        {
            currentSong = videoInfo;
        }
        
        public async void PlayMusic()
        {
            mainWindow.status.Text = "Loading...";
            await GetSongStream();
            OnSongChange?.Invoke(this, null);
            output?.Dispose();
            output.Init(wave);
            output.Play();
            mainWindow.status.Text = "Playing";
        }

        public void PauseMusic()
        {
            output.Pause();
            mainWindow.status.Text = "Paused";
            mainWindow.playpause.Source= new BitmapImage(
            new Uri($"{Environment.CurrentDirectory}\\Images\\_Play.png"));
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
            OnPositionChange?.Invoke(this, null);
        }
    }
}
