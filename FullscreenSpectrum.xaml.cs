
using System;
using System.Collections.Generic;

using System.Linq;

using System.Text.RegularExpressions;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows.Threading;


using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for FullscreenSpectrum.xaml
    /// </summary>
    public partial class FullscreenSpectrum : System.Windows.Window
    {


        int thresholdIndex = 50;
        int multiplierLow = 2000;
        int multiplierHigh = 1000000;
        float decreaseRateFactor = 1.2f;
        int[] multipliers = new int[256];

        // System.IO.Ports.SerialPort serialPort;

        DispatcherTimer timer2 = new DispatcherTimer();
        DispatcherTimer lyricTimer = new DispatcherTimer();
        DispatcherTimer timer4 = new DispatcherTimer();
        DispatcherTimer timer5 = new DispatcherTimer();

        private DispatcherTimer timer;
        float[] decreaserate = new float[512];
        List<ProgressBar> spectrumBars = new List<ProgressBar>();
        private bool isChosingTimeStap = false;
        MainWindow mainWindow;
        private bool isLyrics = true;
        // Create and set the new BitmapImage
        BitmapImage newImage = new BitmapImage();
        Uri currentUri;


        int thresholdIndex16 = 13;
        int multiplierLow16 = 100;
        int multiplierHigh16 = 100;
        double[] heightestBand = new double[16];
        int[] multipliers16 = new int[16];
        int[] buffer = new int[16];
        int[] snow = new int[16];
        //  LedSpectrum ledSpectrum;

        public FullscreenSpectrum(MainWindow mainWindow)
        {

            InitializeComponent();
            //   serialPort = new SerialPort("COM3", 115200);
            /*try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }*/

            //  ledSpectrum = new LedSpectrum(16, 20);
            this.KeyDown += FullscreenSpectrum_KeyDown;
            // Closed += FullscreenSpectrum_Closed;
            this.mainWindow = mainWindow;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);

            timer.Tick += async (sender, e) =>
            {
                await UpadateSpectrum();
            };
            this.mainWindow.MediaPlayer.OnSongChange += MainWindow_OnSongChange;
            CreateSpectrumBar();

            for (int i = 0; i < 256; i++)
            {

                multipliers[i] = i < thresholdIndex ? multiplierLow : multiplierHigh;
                multipliers[i] = i < 10 ? 600 : multipliers[i];
                multipliers[i] = 1;
            }
            for (int i = 0; i < 16; i++)
            {

                multipliers16[i] = i < thresholdIndex16 ? multiplierLow16 : multiplierHigh16;
                heightestBand[i] = 1;

            }

            Reopen();

            timer2.Interval = TimeSpan.FromSeconds(5);
            timer2.Tick += Timer_Tick;

            lyricTimer.Interval = TimeSpan.FromMilliseconds(100);
            lyricTimer.Tick += async (sender, e) =>
            {
                await LyricsUpdate();
            };

            timer4.Interval = TimeSpan.FromMilliseconds(1);
            timer4.Tick += UpadateSpectrumBar;

            timer5.Interval = TimeSpan.FromMilliseconds(1);
            timer5.Tick += UpadateSpectrumUpBar;

            // timer6.Interval = TimeSpan.FromMilliseconds(20);
            // timer6.Tick += UpadateLed;


            this.MouseMove += FullscreenSpectrum_MouseMove;
        }

        private async Task LyricsUpdate()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (isLyrics)
                    {
                        double seconds = mainWindow.MediaPlayer.Wave.CurrentTime.TotalSeconds - MusicSetting.lyricsOffset;
                        var line = mainWindow.MediaPlayer.CurrentSong.GetLyricBytime(seconds);
                        if (line.Length > 10)
                        {
                            this.lyric.FontSize = 60;
                        }
                        else if (line.Length > 12)
                        {
                            this.lyric.FontSize = 50;
                        }
                        else
                        {
                            this.lyric.FontSize = 72;
                        }
                        this.lyric.Text = line;
                        this.postLyric.Text = "";
                    }
                    else
                    {
                        this.lyric.Text = "";
                        this.postLyric.Text = "";
                    }
                    

                });
            });
        }

        private void UpadateLed(object sender, EventArgs e)
        {


            String data = "";
            for (int i = 0; i < 15; i++)
            {

                data += $"{buffer[i]} ";
            }



            //  serialPort.Write(data);
            //ledSpectrum.DrawSpectrum(buffer, snow);

        }
        private void CreateSpectrumLed()
        {
            // spectrum_ctn.Children.Add(ledSpectrum.LedStripContainer);
        }
        private void FullscreenSpectrum_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            Cursor = System.Windows.Input.Cursors.Arrow;
            if (controler.Width == 0)
                controler.Width = Double.NaN;
            timer2.Stop();
            timer2.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            controler.Width = 0;
            Console.WriteLine("hit 0");
            Cursor = System.Windows.Input.Cursors.None;
            // Stop the timer
            timer2.Stop();
        }

        private void FullscreenSpectrum_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.MouseMove -= FullscreenSpectrum_MouseMove;
                mainWindow.WindowState = WindowState.Normal;
                timer.Stop();
                thumbnail.ImageSource = null;
                newImage = new BitmapImage(new Uri("https://i.stack.imgur.com/Pg49k.png"));
                thumbnail.ImageSource = newImage;
                try { newImage.StreamSource.Dispose(); } catch { }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                mainWindow.UIControl.CloseFullScreen();
            }
        }


        private void MainWindow_OnSongChange(object sender, EventArgs e)
        {
            UpdateVisual();
        }
        public void Reopen()
        {
            timer.Start();
            lyricTimer.Start();
            // timer4.Start();
            // timer5.Start();
            // timer6.Start();
            if (currentUri != null)
            {
                newImage = new BitmapImage(currentUri);
                thumbnail.ImageSource = newImage;
                try { newImage.StreamSource.Dispose(); } catch { }
            }
            this.MouseMove += FullscreenSpectrum_MouseMove;
            this.Topmost = true;
            this.mainWindow.WindowState = WindowState.Minimized;
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
            if (!isLyrics) return;

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
                    DrawGraph();
                });
            });
        }

        bool isIncrese = false;
        private void UpadateSpectrumBar(object sender, EventArgs e)
        {

            if (isIncrese) return;

            for (int i = 0; i < 15; i++)
            {
                if (buffer[i] > 0)
                {
                    buffer[i] -= 1;
                }

            }

            //   ledSpectrum.DrawSpectrum(buffer, snow);

        }
        private void UpadateSpectrumUpBar(object sender, EventArgs e)
        {

            /*           for(int i = 0; i < 15; i++)
                       {
                           double average = 0;
                           for (int j = 0; j < 6; j++)
                           {

                               average += mainWindow.DynamicVisualUpdate.Visualizer.fbands[i*6+j];
                           }
                               average /= 6;
                           if (heightestBand[i] < average)
                           {
                               heightestBand[i] = average;
                           }
                           if (average == 0) heightestBand[i] = 0;
                           var spectrumValue = (average / heightestBand[i]) * 19;
                           if(spectrumValue < 0) spectrumValue = 1;
                           spectrumValue = (int)spectrumValue;
                           if (spectrumValue >= buffer[i]||true)
                           {

                               buffer[i] = (int)spectrumValue;
                               if (snow[ i] < buffer[i])
                               {
                                   snow[i] = buffer[i] - 1;
                               }

                           }

                       }*/

            isIncrese = true;
            for (int i = 0; i < 8; i++)
            {

                double average = 0;
                for (int j = 0; j <= Math.Pow(2, i); j++)
                {

                    average += mainWindow.DynamicVisualUpdate.Visualizer.fbands[(int)(2 * (Math.Pow(2, i) - 1 + j))];
                    //Console.WriteLine((int)(2 * (Math.Pow(2, i) - 1 + j)));
                }

                average /= Math.Pow(2, i);
                if (heightestBand[2 * i] < average)
                {
                    heightestBand[2 * i] = average;
                }
                // if (average == 0) heightestBand[2 * i] = 50;
                var spectrumValue = (average / heightestBand[2 * i]) * 19;
                spectrumValue = Math.Ceiling(spectrumValue);
                if (spectrumValue < 0) spectrumValue = 0;
                if (spectrumValue > 19) spectrumValue = 19;
                Console.WriteLine(spectrumValue);
                if (spectrumValue > buffer[2 * i] || true)
                {

                    buffer[2 * i] = (int)spectrumValue;
                    if (snow[2 * i] < buffer[2 * i])
                    {
                        snow[2 * i] = buffer[2 * i] - 1;
                    }



                }
                else
                {
                    buffer[2 * i] -= 1;
                }

                average = 0;
                for (int j = 0; j < Math.Pow(2, i); j++)
                {
                    //Console.WriteLine((int)(2 * (Math.Pow(2, i) - 1 + j)) + 1);
                    average += mainWindow.DynamicVisualUpdate.Visualizer.fbands[(int)(2 * (Math.Pow(2, i) - 1 + j)) + 1];
                }
                if (heightestBand[2 * i + 1] < average)
                {
                    heightestBand[2 * i + 1] = average;
                }
                //   if (average == 0) heightestBand[2 * i + 1] = 50;
                spectrumValue = (average / heightestBand[2 * i + 1]) * 19;
                spectrumValue = Math.Ceiling(spectrumValue);
                if (spectrumValue < 0) spectrumValue = 0;
                if (spectrumValue > 19) spectrumValue = 19;
                Console.WriteLine(spectrumValue);
                if (spectrumValue > buffer[2 * i + 1] || true)
                {

                    buffer[2 * i + 1] = (int)spectrumValue;
                    if (snow[2 * i + 1] < buffer[2 * i + 1])
                    {
                        snow[2 * i + 1] = buffer[2 * i + 1] - 1;
                    }


                }
                else
                {
                    buffer[2 * i + 1] -= 1;

                }
            }

            Console.WriteLine("----------------");
            isIncrese = false;
            //  ledSpectrum.DrawSpectrum(buffer, snow);
        }

        private void DrawGraph()
        {

            int positiveThreshhold = 70;
            for (int i = 0, j = 0; i < 256; i++)
            {
                float multiplier = multipliers[i];
                var fbands = mainWindow.DynamicVisualUpdate.Visualizer.fbands;

                if ((fbands[i] + positiveThreshhold) / positiveThreshhold > spectrumBars[i].Value)
                {
                    spectrumBars[i].Value = (((fbands[i] + positiveThreshhold)) / positiveThreshhold) * 1.5f; // (fbands[i]) * multiplier; //((fbands[nextIndex] + fbands[nextIndex + 1]) / 2) * multiplier;
                    //spectrumBars[i].Value = (((fbands[nextIndex] + positiveThreshhold + fbands[nextIndex + 1] + positiveThreshhold) / 2) / 60) * 1.5f * multiplier;
                    decreaserate[i] = 0.005f;
                }
                else
                {
                    if (spectrumBars[i].Value > 0)
                    {
                        spectrumBars[i].Value -= 1 * decreaserate[i];
                        decreaserate[i] *= decreaseRateFactor;
                    }
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

            thumbnail.ImageSource = null;


            try { newImage.StreamSource.Dispose(); } catch { }

            Console.WriteLine("G");
            currentUri = new Uri(background_mod.Text);
            newImage = new BitmapImage(currentUri);


            // Set the new BitmapImage as the source
            thumbnail.ImageSource = newImage;
            try { newImage.StreamSource.Dispose(); } catch { }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        private void change_background_local(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg;*.gif)|*.png;*.jpeg;*.jpg;*.gif|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            thumbnail.ImageSource = null;
            currentUri = new Uri(openFileDialog.FileName);
            newImage = new BitmapImage(currentUri);
            thumbnail.ImageSource = newImage;
            try { newImage.StreamSource.Dispose(); } catch { }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
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
                //  timer3.Interval = TimeSpan.FromMilliseconds((int)(sender as Slider).Value);
                foreach (var bar in spectrumBars)
                {
                    bar.Width = (int)(sender as Slider).Value;
                }
                if (width_info != null)
                    width_info.Text = ((sender as Slider).Value).ToString();
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
                //  timer4.Interval = TimeSpan.FromMilliseconds((int)(sender as Slider).Value);
                foreach (var bar in spectrumBars)
                {

                    bar.Height = (int)(sender as Slider).Value;

                }
                if (height_info != null)
                    height_info.Text = ((sender as Slider).Value).ToString();
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
                //  timer5.Interval = TimeSpan.FromMilliseconds((int)(sender as Slider).Value);
                foreach (var bar in spectrumBars)
                {

                    bar.Margin = new Thickness(0, 0, (int)(sender as Slider).Value, 0);
                }
                if (space_info != null)
                    space_info.Text = ((sender as Slider).Value).ToString();
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
                    Width = 2,
                    Margin = new Thickness(0, 0, 6, 0),
                    Height = 180,
                    Maximum = 1,
                    Orientation = Orientation.Vertical,
                    Value = 0,

                };


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
        /* private void ReleaseMemory()
         {
             multipliers = null;
             thresholdIndex = 0;
             multiplierLow = 0;
             multiplierHigh = 0;
             decreaseRateFactor = 0;

             this.opacity_info = null;
             this.postLyric = null;
             this.setting = null;
             this.songValue = null;
             this.space_info = null;
             this.spectrumBars = null;
             this.spectrum_ctn = null;
             this.stopresumeimg = null;
             this.thresholdIndex = 0;
             this.thumb = null;
             this.thumbnail = null;
             this.timer = null;
             this.volum = null;
             this.volumVisual = null;
             this.width_info = null;
             this.KeyDown -= FullscreenSpectrum_KeyDown;
             //this.mainWindow= null;
             this.artist_cover = null;
             this.background_mod = null;
             this.decreaserate = null;
             this.decreaseRateFactor = 0;
             this.des = null;
             this.filter = null;
             this.height_info = null;
             this.lable = null;
             this.loop_img = null;
             this.lyric = null;


         }*/
    }

}
