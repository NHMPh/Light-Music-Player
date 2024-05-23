using Accord.Math;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NHMPh_music_player
{
    public class UIControl
    {
        MainWindow mainWindow;
        MediaPlayer mediaPlayer;
        SongsManager songsManager;
        FullscreenSpectrum fullscreenSpectrum =null;
        ArdunoSetting ardunoSetting = null;
        public UIControl(MainWindow mainWindow, MediaPlayer mediaPlayer, SongsManager songsManager)
        {
            this.mainWindow = mainWindow;
            this.mediaPlayer = mediaPlayer;
            this.songsManager = songsManager;

             

            mainWindow.autoplay_btn.Click += AutoplayBtn;
            mainWindow.close_btn.Click += CloseBtn;

            mainWindow.arduno_btn.Click += ArdunoBtn;

            mainWindow.lyricsSync_btn.Click += SyncLyricsBtn;
            mainWindow.lyrics_btn.Click += LyricBtn;
            mainWindow.minimize_btn.Click += MinimizeBtn;
            mainWindow.spectrum_btn.Click += SpectrumBtn;
            mainWindow.loopBtn.Click += LoopBtn;
            mainWindow.pauseBtn.Click += PauseResumeBtn;
            mainWindow.skipBtn.Click += SkipBtn;
            mainWindow.spectrum_btn.MouseRightButtonDown += FullSpectrumBtn;

            mainWindow.Track.MouseLeftButtonDown += Thumb_MouseLeftButtonDown;
            mainWindow.thumb.GotMouseCapture += Thumb_GotMouseCapture;
            mainWindow.thumb.LostMouseCapture += Thumb_LostMouseCapture;

            mainWindow.volume.ValueChanged += Volume_ValueChanged;

            mainWindow.searchBar.KeyDown += SearchBarEnterKeyDown;


            mainWindow.Closed += MainWindow_Closed;
            mainWindow.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;

            mainWindow.Topmost = true;
            mainWindow.Icon = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\icon.ico"));
        }

        private void ArdunoBtn(object sender, RoutedEventArgs e)
        {

            if (ardunoSetting == null)
            {
               ardunoSetting = new ArdunoSetting(mainWindow.DynamicVisualUpdate.Visualizer);
            }
            ardunoSetting.Show();


        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.DragMove();
        }



        private void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                window.Close();
            }
        }


        #region Button control
        public void PauseResumeBtn(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.PlaybackState == PlaybackState.Playing)
                mediaPlayer.PauseMusic();
            else if (mediaPlayer.PlaybackState == PlaybackState.Paused)
                mediaPlayer.ResumeMusic();
        }
        public void SkipBtn(object sender, RoutedEventArgs e)
        {
            if (songsManager.TotalSongInQueue() == 0)
            {
                mediaPlayer.Seek(0);
                return;
            }
            songsManager.NextSong();
            mediaPlayer.CurrentSong = songsManager.CurrentSong;
            mediaPlayer.PlayMusic();
        }
        public void LoopBtn(object sender, RoutedEventArgs e)
        {
            MusicSetting.isLoop = !MusicSetting.isLoop;
            mainWindow.loopTxt.Text = MusicSetting.isLoop ? " on " : " off ";
        }
        public void SyncLyricsBtn(object sender, RoutedEventArgs e)
        {
            if (MusicSetting.lyricsOffset == 0)
            {
                MusicSetting.lyricsOffset = (int)mediaPlayer.Wave.CurrentTime.TotalSeconds - int.Parse(mediaPlayer.CurrentSong.SongLyrics[0]["seconds"].ToString());
            }
            else
            {
                mediaPlayer.Seek(0);
                MusicSetting.lyricsOffset = 0;
            }
        }
        public void LyricBtn(object sender, RoutedEventArgs e)
        {
            MusicSetting.isLyrics = !MusicSetting.isLyrics;
            if (MusicSetting.isLyrics)
                mainWindow.description.Text = "For more accurate lyrics sync, search \"[song name] + lyrics.\"";
            else
                mainWindow.description.Text = mediaPlayer.CurrentSong.Description;
        }
        private void AutoplayBtn(object sender, RoutedEventArgs e)
        {
            if (MusicSetting.isBrowser)
            {
                MessageBox.Show("Task in progress please wait!");
                return;
            }
            if (songsManager.CurrentSong == null)
            {
                MessageBox.Show("Play any song to use autoplay");
                return;
            }
            MusicSetting.isAutoPlay = !MusicSetting.isAutoPlay;
            mainWindow.auto_txt.Text = MusicSetting.isAutoPlay ? " on " : " off ";


            if (MusicSetting.isAutoPlay)
            {
                if (songsManager.VideoInfosAutoPlayQueue.Count == 0)
                {

                    songsManager.AddSongAutoplay(mainWindow);
                }
            }
            else
            {

                songsManager.RemoveSongAutoPlay();

            }
        }

        private void CloseBtn(object sender, RoutedEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                window.Close();
            }
        }       
        public void SpectrumBtn(object sender, RoutedEventArgs e)
        {
            MusicSetting.isSpectrum = !MusicSetting.isSpectrum;

            if (!MusicSetting.isSpectrum)
            {
                mainWindow.description.Height = 76;
                mainWindow.spectrum_ctn.Height = 0;
                mainWindow.DynamicVisualUpdate.Visualizer.fbands.Clear();

            }
            if (MusicSetting.isSpectrum)
            {
                mainWindow.description.Height = 16;
                mainWindow.spectrum_ctn.Height = 60;
                //  UpdateGraph();
                //  DrawGraph();
            };
        }
        private void MinimizeBtn(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }
        private void FullSpectrumBtn(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Press");
            if (!MusicSetting.isFullScreen)
            {
                if(fullscreenSpectrum == null)
                {
                    fullscreenSpectrum = new FullscreenSpectrum(mainWindow);
                }
                
                MusicSetting.isFullScreen = true;
                fullscreenSpectrum.Show();
                fullscreenSpectrum.Reopen();
                fullscreenSpectrum.UpdateVisual();
            }

        }
        public async void CloseFullScreen()
        {
            MusicSetting.isFullScreen = false;
           
            fullscreenSpectrum.Hide();
            fullscreenSpectrum.Show();    
            fullscreenSpectrum.Hide();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        #endregion

        public void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = (float)mainWindow.volume.Value;
            mainWindow.volumVisual.Value = mainWindow.volume.Value;
        }
        public void ChangeVolume(Slider volume, ProgressBar volumeVisual)
        {
            mediaPlayer.Volume = (float)volume.Value;
            volumeVisual.Value = volume.Value;
        }
        private void Thumb_GotMouseCapture(object sender, MouseEventArgs e)
        {
            MusicSetting.isChosingTimeStap = true;
        }
        private void Thumb_LostMouseCapture(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Drop");
            MusicSetting.isChosingTimeStap = false;
            int second = (int)Math.Floor(mainWindow.thumb.Value / 1000);
            mediaPlayer.Seek(second);
        }
        private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = Mouse.GetPosition(mainWindow.songProgress);
            Console.WriteLine("Mouse position: X = " + position.X + ", Y = " + position.Y);
            double mousePositionPercentage = position.X / mainWindow.songProgress.Width;
            double thumbPostion = mainWindow.thumb.Maximum * mousePositionPercentage;
            int second = (int)Math.Floor(thumbPostion / 1000);
            mediaPlayer.Seek(second);
        }
        private async void SearchBarEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            string key = (sender as TextBox).Text;
            (sender as TextBox).Text = "";
            await Searcher.Search(key, songsManager);

            if (mediaPlayer.PlaybackState == PlaybackState.Stopped)
            {
                songsManager.NextSong();
                mediaPlayer.CurrentSong = songsManager.CurrentSong;
                Console.WriteLine(mediaPlayer.CurrentSong);
                mediaPlayer.PlayMusic();

            }
        }

    }
}
