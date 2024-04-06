using AngleSharp.Media;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDLSharp.Options;
using YoutubeDLSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Threading;
using System.IO;
using System.Runtime.CompilerServices;

namespace NHMPh_music_player
{
    internal class MediaPlayer
    {
        private WaveOutEvent output = new WaveOutEvent();
        private MediaFoundationReader _mf;
        private WaveChannel32 wave;

        private VideoInfo currentSong;

        public VideoInfo CurrentSong { get { return currentSong; } set { currentSong = value; } }
        public MediaPlayer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await TrackManager();
            };
        }
        private async void GetSongStream()
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
            Console.WriteLine(streamUrl.Data[0]);
            //set _mf for playing audio
            _mf = new MediaFoundationReader(streamUrl.Data[0]);
            wave = new WaveChannel32(_mf);
        }
        public MediaPlayer(VideoInfo videoInfo)
        {
            currentSong = videoInfo;
        }

        public void PlayMusic()
        {
            output?.Dispose();
            output.Init(wave);
            output.Play();
        }

        public void PauseMusic()
        {

        }
        public void ResumeMusic()
        {

        }
        public void SkipMusic()
        {

        }

        public void Seek(int seconds)
        {

        }
        private async Task TrackManager()
        {


            await Task.Run(() =>
            {
                
            });
        }
    }
}
