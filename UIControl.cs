using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NHMPh_music_player
{
    internal class UIControl
    {
        MainWindow mainWindow;
        MediaPlayer player;
        SongsManager songManager;
        public UIControl(MainWindow mainWindow, MediaPlayer mediaPlayer, SongsManager songsManager)
        { 
            this.mainWindow = mainWindow;
            this.player = mediaPlayer;
            this.songManager = songManager;


            mainWindow.autoplay_btn.Click += AutoplayBtn;
            mainWindow.close_btn.Click += CloseBtn;
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
        }


        #region Button control
        private void PauseResumeBtn(object sender, RoutedEventArgs e)
        {

        }
        private void SkipBtn(object sender, RoutedEventArgs e)
        {

        }
        private void LoopBtn(object sender, RoutedEventArgs e)
        {

        }
        private void SyncLyricsBtn(object sender, RoutedEventArgs e)
        {

        }
        private void LyricBtn(object sender, RoutedEventArgs e)
        {

        }
        private void AutoplayBtn(object sender, RoutedEventArgs e)
        {
           
        }
        private void ViewCustomBtn(object sender, RoutedEventArgs e)
        {

        }
        private void CloseBtn(object sender, RoutedEventArgs e)
        {

        }
        private void SpectrumBtn(object sender, RoutedEventArgs e)
        {

        }
        private void MinimizeBtn(object sender, RoutedEventArgs e)
        {

        }
        private void FullSpectrumBtn(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
        }
        private void SongProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }
        private void Thumb_GotMouseCapture(object sender, MouseEventArgs e)
        {
           
        }
        private void Thumb_LostMouseCapture(object sender, MouseEventArgs e)
        {
           
        }
        private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }
        private async void SearchBarEnterKeyDown(object sender, KeyEventArgs e)
        {

        }

    }
}
