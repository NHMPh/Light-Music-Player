﻿using System;
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
        private DispatcherTimer lyricTimer;
        private DispatcherTimer spectrumTimer;
        private MainWindow window;
        private MediaPlayer mediaPlayer;
        private SongsManager songsManager;
        private SpectrumVisualizer visualizer;
        private StaticVisualUpdate staticVisualUpdate;

        public SpectrumVisualizer Visualizer { get { return visualizer; } }
        public DynamicVisualUpdate(MainWindow mainWindow, MediaPlayer mediaPlayer, SongsManager songsManager, SpectrumVisualizer visualizer)
        {
            window = mainWindow;
            staticVisualUpdate = new StaticVisualUpdate(window);
            this.songsManager = songsManager;
            this.mediaPlayer = mediaPlayer;
            this.mediaPlayer.OnSongChange += MediaPlayer_OnSongChange;
            this.songsManager.OnVideoQueueChange += SongsManager_OnVideoQueueChange;
            // visualizer = new SpectrumVisualizer(window, mediaPlayer);
            this.visualizer = visualizer;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await Update();
            };
            timer.Start();

            lyricTimer = new DispatcherTimer();
            lyricTimer.Interval = TimeSpan.FromSeconds(0.01); // Set the interval as needed
            lyricTimer.Tick += async (sender, e) =>
            {
                await LyricsUpdate();
            };

            lyricTimer.Start();
            spectrumTimer = new DispatcherTimer();
            spectrumTimer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            spectrumTimer.Tick += async (sender, e) =>
            {
                await SpectrumUpdate();
            };
            spectrumTimer.Start();
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

            if (MusicSetting.isRadio) return;
            songsManager.InvokeVideoQueueChange();
        }
        private void UpdateStaticVisual()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                staticVisualUpdate.SetVisual(mediaPlayer.CurrentSong);
            });

            UpdateTrackBarVisual();
            if (MusicSetting.isRadio) return;
            mediaPlayer.CurrentSong.GetLyrics(window, window.youtube);
            mediaPlayer.CurrentSong.GetFullDescription(staticVisualUpdate, window.youtube);
        }
        private void UpdateTrackBarVisual()
        {
            window.songProgress.Value = 0;
            window.thumb.Value = 0;
            window.songProgress.Maximum = mediaPlayer.Wave.TotalTime.TotalMilliseconds;
            window.thumb.Maximum = window.songProgress.Maximum;
        }

        private async Task LyricsUpdate()
        {
            await Task.Run(() =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (MusicSetting.isLyrics)
                    {

                        try
                        {
                            if (mediaPlayer.CurrentSong._SongLyrics.Captions.Count > 0)
                            {

                                double seconds = mediaPlayer.Wave.CurrentTime.TotalSeconds - MusicSetting.lyricsOffset;
                                var caption = mediaPlayer.CurrentSong._SongLyrics.GetByTime(TimeSpan.FromSeconds(seconds));                           
                                window.description.Text = caption.Text.Replace("\n", " ");
                            }
                        }catch (Exception ex) { }
                           
                       

                        if (mediaPlayer.CurrentSong.SongLyrics.Count > 0)
                        {

                            foreach (var lyric in mediaPlayer.CurrentSong.SongLyrics)
                            {

                              
                                if (lyric.Item1 / 1000 == ((int)mediaPlayer.Wave.CurrentTime.TotalSeconds - MusicSetting.lyricsOffset))
                                {
                                    window.description.Text = lyric.Item2.ToString().Replace("\n", " ");
                                    if (lyric.Item2.ToString() == "")
                                        window.description.Text = "[Music]";
                                }



                            }
                        }
                    }
                });
            });
        }
        private async Task SpectrumUpdate()
        {
            await Task.Run(() =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (mediaPlayer.PlaybackState == PlaybackState.Playing && MusicSetting.isSpectrum)
                    {
                        visualizer.UpadateSpectrumBar15();
                        if (!MusicSetting.isFullScreen)
                            visualizer.DrawGraph();
                    }
                });
            });
        }
        private async Task Update()
        {
            await Task.Run(() =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (mediaPlayer.Wave == null) return;
                    if (MusicSetting.isRadio) return;
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



                        }
                    }

                });
            });
        }

    }
}
