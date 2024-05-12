using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Math.Distances;
using NAudio.Wave;
using static System.Net.Mime.MediaTypeNames;

namespace NHMPh_music_player
{
    public class DynamicVisualUpdate
    {
        private DispatcherTimer timer;
        private MainWindow window;
        private MediaPlayer mediaPlayer;
        private SongsManager songsManager;
        private SpectrumVisualizer visualizer;
        private StaticVisualUpdate staticVisualUpdate;

        public SpectrumVisualizer Visualizer { get { return visualizer; } }
        public DynamicVisualUpdate(MainWindow mainWindow, MediaPlayer mediaPlayer, SongsManager songsManager)
        {
            window = mainWindow;
            staticVisualUpdate = new StaticVisualUpdate(window);
            this.songsManager = songsManager;
            this.mediaPlayer = mediaPlayer;
            this.mediaPlayer.OnSongChange += MediaPlayer_OnSongChange;
            this.songsManager.OnVideoQueueChange += SongsManager_OnVideoQueueChange;
            visualizer = new SpectrumVisualizer(window, mediaPlayer);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await Update();
            };
            timer.Start();
        }
        private void SongsManager_OnVideoQueueChange(object sender, EventArgs e)
        {
            window.queue_txt.Text = $" {songsManager.TotalSongInQueue().ToString()} ";
            window.next_txt.Text = songsManager.VideoInfosQueue.Count != 0 ? $"{songsManager.VideoInfosQueue.ElementAt(0).Title}" : songsManager.VideoInfosAutoPlayQueue.Count != 0 ? $"{songsManager.VideoInfosAutoPlayQueue.ElementAt(0).Title}" : $"Click to select song from queue";

        }

        private void MediaPlayer_OnSongChange(object sender, EventArgs e)
        {
            window.lyricsSync_btn.Width = 0;
            window.lyrics_btn.Width = 0;
            MusicSetting.lyricsOffset = 0;
            UpdateStaticVisual();
            visualizer.SetSpectrumWave();

            songsManager.InvokeVideoQueueChange();
        }
        private void UpdateStaticVisual()
        {
            staticVisualUpdate.SetVisual(mediaPlayer.CurrentSong);
            UpdateTrackBarVisual();
            mediaPlayer.CurrentSong.GetLyrics(window);
            mediaPlayer.CurrentSong.GetFullDescription(staticVisualUpdate);
        }
        private void UpdateTrackBarVisual()
        {
            window.songProgress.Value = 0;
            window.thumb.Value = 0;
            window.songProgress.Maximum = mediaPlayer.Wave.TotalTime.TotalMilliseconds;
            window.thumb.Maximum = window.songProgress.Maximum;
        }
        private async Task Update()
        {
            await Task.Run(() =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (mediaPlayer.Wave == null) return;
                    //Song end
                    if (window.songProgress.Value == mediaPlayer.Wave.TotalTime.TotalMilliseconds)
                    {

                        //When loop is on
                        if (MusicSetting.isLoop)
                        {

                            UpdateTrackBarVisual();
                            mediaPlayer.Seek(0);
                        }
                        else
                        {
                            if (songsManager.TotalSongInQueue() != 0)
                            {
                                window.lyricsSync_btn.Width = 0;
                                window.lyrics_btn.Width = 0;
                                window.songProgress.Value = 0;
                                window.thumb.Value = 0;
                                mediaPlayer.Wave = null;
                                songsManager.NextSong();
                                mediaPlayer.CurrentSong = songsManager.CurrentSong;
                                mediaPlayer.PlayMusic();
                            }
                            else
                            {
                                mediaPlayer._PauseMusic();
                            }

                            if (MusicSetting.isAutoPlay && songsManager.VideoInfosAutoPlayQueue.Count <= 1 && !MusicSetting.isBrowser)
                            {
                                songsManager.AddSongAutoplay(window);
                            }
                        }
                    }
                    else
                    {
                        if (mediaPlayer.Wave != null)
                        {
                           
                            window.songProgress.Value = mediaPlayer.Wave.CurrentTime.TotalMilliseconds;
                            if (!MusicSetting.isChosingTimeStap)
                            {
                                window.thumb.Value = window.songProgress.Value;
                            }
                            if (mediaPlayer.PlaybackState == PlaybackState.Playing && MusicSetting.isSpectrum)
                            {

                                visualizer.UpdateGraph();
                                

                                // if (!MusicSetting.isFullScreen)
                                 visualizer.DrawGraph();
                            }
                            if (MusicSetting.isLyrics)
                            {
                                foreach (var lyric in mediaPlayer.CurrentSong.SongLyrics)
                                {
                                    if (lyric["seconds"].ToString() == ((int)mediaPlayer.Wave.CurrentTime.TotalSeconds - MusicSetting.lyricsOffset).ToString())
                                    {
                                        window.description.Text = lyric["lyrics"].ToString();
                                    }
                                }

                            }

                        }
                    }

                });
            });
        }

    }
}
