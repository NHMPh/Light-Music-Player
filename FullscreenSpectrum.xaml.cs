﻿using NAudio.Wave;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using YoutubeExplode.Common;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for FullscreenSpectrum.xaml
    /// </summary>
    public partial class FullscreenSpectrum : Window
    {

        const int numBars = 256;
        const int thresholdIndex = 50;
        const int multiplierLow = 2000;
        const int multiplierHigh = 5000;
        const float decreaseRateFactor = 1.3f;
        int[] multipliers = new int[numBars];


        private DispatcherTimer timer;
        float[] decreaserate = new float[512];
        List<ProgressBar> spectrumBars = new List<ProgressBar>();
        private bool isChosingTimeStap = false;
        MainWindow mainWindow;
        private bool isLyrics = true;
        public FullscreenSpectrum(MainWindow mainWindow)
        {

            InitializeComponent();
            this.KeyDown += FullscreenSpectrum_KeyDown;
            this.mainWindow = mainWindow;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await UpadateSpectrum();
            };
            mainWindow.MediaPlayer.OnSongChange += MainWindow_OnSongChange;
            CreateSpectrumBar();
            Closed += FullscreenSpectrum_Closed;
            timer.Start();
            this.Topmost = true;
            for (int i = 0; i < numBars; i++)
            {
                multipliers[i] = i < thresholdIndex ? multiplierLow : multiplierHigh;
                multipliers[i] = i < 10 ? 600 : multipliers[i];
            }
            mainWindow.WindowState = WindowState.Minimized;
        }

        private void FullscreenSpectrum_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                mainWindow.WindowState = WindowState.Normal;
                this.Close();

            }
        }

        private void FullscreenSpectrum_Closed(object sender, EventArgs e)
        {
            MusicSetting.isFullScreen = false;
            Console.WriteLine("Close");
        }

        private void MainWindow_OnSongChange(object sender, EventArgs e)
        {
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            
            lable.Content = Regex.Replace(mainWindow.MediaPlayer.CurrentSong.Title, @"(\([^)]*\)|\[[^\]]*\])|ft\..*|FT\..*|Ft\..*|feat\..*|Feat\..*|FEAT\..*|【|】", ""); ;
            des.Content = mainWindow.MediaPlayer.CurrentSong.Description;
            artist_cover.ImageSource = new BitmapImage(new Uri(mainWindow.MediaPlayer.CurrentSong.Thumbnail));
            songValue.Value = 0;
            songValue.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
            thumb.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
            volum.Value = mainWindow.volume.Value;
            volumVisual.Value = mainWindow.volumVisual.Value;

            mainWindow.MediaPlayer.CurrentSong.OnLyricsFound -= CurrentSong_OnLyricsFound;
            mainWindow.MediaPlayer.CurrentSong.OnLyricsFound += CurrentSong_OnLyricsFound;

        }

        private void CurrentSong_OnLyricsFound(object sender, bool status)
        {

            if (status)
            {
                this.lyric.Text = "INTERMISSION";
                postLyric.Text = "For more accurate lyrics sync, search \"[song name] + lyrics.\" or use sync lyrics button";
            }
            else
            {
                this.lyric.Text = "Lyrics Not Found";
                postLyric.Text = "";
            }
        }

        private async Task UpadateSpectrum()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (this.Topmost) this.Topmost = false;
                    if (mainWindow.MediaPlayer.Wave == null) return;

                    songValue.Value = mainWindow.MediaPlayer.Wave.CurrentTime.TotalMilliseconds;

                    
                    if (mainWindow.MediaPlayer.CurrentSong.SongLyrics != null)
                    {
                        foreach (var lyric in mainWindow.MediaPlayer.CurrentSong.SongLyrics)
                        {
                            if (lyric["seconds"].ToString() == ((int)mainWindow.MediaPlayer.Wave.CurrentTime.TotalSeconds - MusicSetting.lyricsOffset).ToString())
                            {
                                if (lyric["lyrics"].ToString().Length > 10)
                                {
                                    this.lyric.FontSize = 60;
                                }
                                else
                                {
                                    this.lyric.FontSize = 72;
                                }

                                if (isLyrics)
                                {

                                    this.lyric.Text = lyric["lyrics"].ToString().Replace("\r", "");

                                    try { postLyric.Text = lyric.Next["lyrics"].ToString(); } catch { postLyric.Text = null; };
                                    //  try { preLyric.Text = lyric.Previous["lyrics"].ToString(); } catch { preLyric.Text = null; };
                                }

                            }
                        }
                    }
                   
                    
                    DrawGraph();

                });
            });
        }
        private void DrawGraph()
        {
            for (int i = 0, j = 0; i < 256; i++)
            {

                if (mainWindow.DynamicVisualUpdate.Visualizer.fbands[i] * multipliers[i] > spectrumBars[i].Value)
                {


                    spectrumBars[i].Value = ((mainWindow.DynamicVisualUpdate.Visualizer.fbands[i + j] + mainWindow.DynamicVisualUpdate.Visualizer.fbands[i + j + 1]) / 2) * multipliers[i];
                    j++;
                    decreaserate[i] = 10f;
                }
                else
                {
                    spectrumBars[i].Value -= 1 * decreaserate[i];
                    decreaserate[i] *= decreaseRateFactor;
                }
            }
        }
        private void change_bars_color(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var bar in spectrumBars)
                {
                    bar.Foreground = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                }
            }
        }
        private void change_background_color(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));

            }

        }
        private void change_background_url(object sender, RoutedEventArgs e)
        {
            thumbnail.ImageSource = new BitmapImage(new Uri(background_mod.Text));
        }
        private void change_background_local(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg;*.gif)|*.png;*.jpeg;*.jpg;*.gif|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            thumbnail.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName));

        }
        private void change_filter_color(object sender, RoutedEventArgs e)
        {

            try
            {
                System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filter.Color = Color.FromArgb(filter.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

                }
            }
            catch
            {
                MessageBox.Show("Invaild input");
                return;
            }
        }
        private void change_filter_opacity(object sender, RoutedEventArgs e)
        {
            try
            {
                filter.Color = Color.FromArgb(byte.Parse(((int)(sender as Slider).Value).ToString()), filter.Color.R, filter.Color.G, filter.Color.B);
                if (opacity_info != null)
                    opacity_info.Text = ((int)(sender as Slider).Value).ToString();
            }
            catch
            {
                // MessageBox.Show("Invaild input");
                return;
            }

        }
        private void change_spectrum_width(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var bar in spectrumBars)
                {
                    bar.Width = (int)(sender as Slider).Value;
                }
                if (width_info != null)
                    width_info.Text = ((int)(sender as Slider).Value).ToString();
            }
            catch
            {
                // MessageBox.Show("Invaild input");
                return;
            }

        }
        private void change_spectrum_height(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var bar in spectrumBars)
                {

                    bar.Height = (int)(sender as Slider).Value;

                }
                if (height_info != null)
                    height_info.Text = ((int)(sender as Slider).Value).ToString();
            }
            catch
            {
                // MessageBox.Show("Invaild input");
                return;
            }

        }
        private void change_spectrum_spaceing(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var bar in spectrumBars)
                {

                    bar.Margin = new Thickness(0, 0, (int)(sender as Slider).Value, 0);
                }
                if (space_info != null)
                    space_info.Text = ((int)(sender as Slider).Value).ToString();
            }
            catch
            {
                //  MessageBox.Show("Invaild input");
                return;
            }

        }
        private void CreateSpectrumBar()
        {
            for (int i = 0; i < 256; i++)
            {

                ProgressBar progressBar = new ProgressBar()
                {
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = new SolidColorBrush(Colors.Cyan),
                    Width = 1,
                    Margin = new Thickness(0, 0, 6, 0),
                    Height = 180,
                    Maximum = 1000,
                    Orientation = Orientation.Vertical,
                    Value = 0,

                };


                //   spectrum_ctn.Children.Add(progressBar);
                spectrumBars.Add(progressBar);
            }

            for (int i = 0; i < 256; i++)
            {
                if (i < 128)
                {
                    spectrum_ctn.Children.Add(spectrumBars[(127 - i) * 2]);

                }
                else
                {
                    spectrum_ctn.Children.Add(spectrumBars[Math.Abs((127 - i) * 2 + 1)]);

                }

            }

        }

        private void songValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isChosingTimeStap)
            {
                thumb.Value = songValue.Value;
            }
        }
        private void thumb_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isChosingTimeStap = true;
        }
        private void thumb_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int second = (int)Math.Floor(thumb.Value / 1000);
            isChosingTimeStap = false;
            mainWindow.MediaPlayer.Seek(second);
        }
        private void thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = Mouse.GetPosition(songValue);
            Console.WriteLine("Mouse position: X = " + position.X + ", Y = " + position.Y);
            double mousePositionPercentage = position.X / songValue.Width;
            double thumbPostion = thumb.Maximum * mousePositionPercentage;
            int second = (int)Math.Floor(thumbPostion / 1000);
            mainWindow.MediaPlayer.Seek(second);
        }
        private void openclose_setting(object sender, RoutedEventArgs e)
        {
            if (setting.Width == 500)
                setting.Width = 0;
            else setting.Width = 500;

        }

        private void skip_btn(object sender, RoutedEventArgs e)
        {
            mainWindow.UIControl.SkipBtn(sender, e);
        }
        private void stop_btn(object sender, RoutedEventArgs e)
        {
            mainWindow.UIControl.PauseResumeBtn(sender, e);
            stopresumeimg.Source = new BitmapImage(
            new Uri($"{Environment.CurrentDirectory}\\Images\\_Play.png"));
            (sender as System.Windows.Controls.Button).Click -= stop_btn;
            (sender as System.Windows.Controls.Button).Click += resume_btn;


        }
        private void resume_btn(object sender, RoutedEventArgs e)
        {
            mainWindow.UIControl.PauseResumeBtn(sender, e);
            stopresumeimg.Source = new BitmapImage(
           new Uri($"{Environment.CurrentDirectory}\\Images\\_Pause.png"));
            (sender as System.Windows.Controls.Button).Click -= resume_btn;
            (sender as System.Windows.Controls.Button).Click += stop_btn;


        }

        private void spectrum_btn(object sender, RoutedEventArgs e)
        {
            mainWindow.UIControl.SpectrumBtn(sender, e);

        }
        private void lyrics_btn(object sender, RoutedEventArgs e)
        {
            isLyrics = !isLyrics;
            if (!isLyrics)
            {
                this.lyric.Text = null;
                postLyric.Text = null;
                //   preLyric.Text = null;
            }
            if (isLyrics)
            {
                if (mainWindow.MediaPlayer.CurrentSong.SongLyrics != null)
                {
                    this.lyric.Text = "INTERMISSION";
                    postLyric.Text = "For more accurate lyrics sync, search \"[song name] + lyrics.\"";
                }
                else
                {
                    this.lyric.Text = "Lyrics Not Found";
                    postLyric.Text = "";
                }
            }
        }
        private void lyricsSync_btn(object sender, RoutedEventArgs e)
        {
            mainWindow.UIControl.SyncLyricsBtn(sender, e);
        }
        private void loop_btn(object sender, RoutedEventArgs e)
        {
            mainWindow.UIControl.LoopBtn(sender, e);
            if (mainWindow.loopTxt.Text == " on ")
            {
                loop_img.Source = new BitmapImage(
                new Uri($"{Environment.CurrentDirectory}\\Images\\Loop_on.png"));
            }
            else
            {
                loop_img.Source = new BitmapImage(
               new Uri($"{Environment.CurrentDirectory}\\Images\\Loop_off.png"));
            }


        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mainWindow.WindowState == WindowState.Normal)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            else
                mainWindow.WindowState = WindowState.Normal;
        }
        private void volum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow != null)
                mainWindow.UIControl.ChangeVolume(volum, volumVisual);
        }
    }

}
