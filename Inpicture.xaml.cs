using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YoutubeExplode;
using System.Windows.Media.Animation;
namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for Inpicture.xaml
    /// </summary>
    public partial class Inpicture : Window
    {
        private readonly VideoInfo _video;
        private readonly YoutubeClient _youtube;

        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer timer1 = new DispatcherTimer();
        private bool isChosingTimeStap = false;
        private bool isLyrics = true;
        private MainWindow mainWindow;
        private BitmapImage newImage = new BitmapImage();
        public event EventHandler OnSongChange;
        public Inpicture(VideoInfo video, MainWindow mainWindow)
        {
            InitializeComponent();
            _video = video;
            _youtube = new YoutubeClient();
            this.mainWindow = mainWindow;
            this.Focus();
            this.MouseDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) this.DragMove(); };  
            this.Loaded += Inpicture_Loaded;

            songValue.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
            thumb.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    OpenMainWindow();
                }
            };
            this.Closing += (s, e) =>
            {
                e.Cancel = true; // Cancel the close
                OpenMainWindow();
            };
            // Attach event handlers for controls
            songValue.ValueChanged += songValue_ValueChanged;
            thumb.GotMouseCapture += thumb_GotMouseCapture;
            thumb.LostMouseCapture += thumb_LostMouseCapture;
            thumb.MouseLeftButtonDown += thumb_MouseLeftButtonDown;
            this.mainWindow.MediaPlayer.OnSongChange += Inpicture_OnSongChange;
        }
        private void OpenMainWindow()
        {
            this.Hide();
            mainWindow.Left = this.Left-280;
            mainWindow.Top = this.Top - 45/2;
            mainWindow.Show();
            MusicSetting.isInPictureMode = false;
        }
        private void Inpicture_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualsAsync();
            // Optionally, set up timer for UI updates if needed
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Inpicture_OnSongChange(object sender, EventArgs e)
        {
            timer1.Interval = TimeSpan.FromMilliseconds(2000);
            timer1.Tick += Timer_Tick2;
            timer1.Start();
           
        }
        private void Timer_Tick2(object sender, EventArgs e)
        {
            var backgroundUrl = mainWindow.MediaPlayer.CurrentSong.Thumbnail;

            BitmapImage bitmap = new BitmapImage();
            using (var webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(backgroundUrl);
                using (var ms = new MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }

            var squareBitmap = CropToSquare(bitmap);
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = squareBitmap;
            this.Background = imageBrush;
            _youtube.Dispose();
            songValue.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
            thumb.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
            timer1.Stop();
        }
        private void Inpicture_Loaded2(object sender, RoutedEventArgs e) { 
        }
        private async void UpdateVisualsAsync()
        {
            var backgroundUrl = _video.Thumbnail;

            BitmapImage bitmap = new BitmapImage();
            using (var webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(backgroundUrl);
                using (var ms = new MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }

            var squareBitmap = CropToSquare(bitmap);
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = squareBitmap;
            this.Background = imageBrush;
            _youtube.Dispose();
            songValue.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
            thumb.Maximum = mainWindow.MediaPlayer.Wave.TotalTime.TotalMilliseconds;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Example: update progress bar if mainWindow is available
            if (mainWindow != null && mainWindow.MediaPlayer != null && mainWindow.MediaPlayer.Wave != null)
            {
                songValue.Value = mainWindow.MediaPlayer.Wave.CurrentTime.TotalMilliseconds;
            }
        }

        public static BitmapSource CropToSquare(BitmapSource source)
        {
            int side = Math.Min(source.PixelWidth, source.PixelHeight);
            int x = (source.PixelWidth - side) / 2;
            int y = (source.PixelHeight - side) / 2;

            var rect = new Int32Rect(x, y, side, side);
            var croppedBitmap = new CroppedBitmap(source, rect);
            return croppedBitmap;
        }

        // Event handlers for UI controls (referenced in XAML)
        private void songValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isChosingTimeStap)
            {
                thumb.Value = songValue.Value;
            }
        }

        private void thumb_GotMouseCapture(object sender, MouseEventArgs e)
        {
            isChosingTimeStap = true;
        }

        private void thumb_LostMouseCapture(object sender, MouseEventArgs e)
        {
            int second = (int)Math.Floor(thumb.Value / 1000);
            isChosingTimeStap = false;
            mainWindow?.MediaPlayer?.Seek(second);
        }

        private void thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = Mouse.GetPosition(songValue);
            double mousePositionPercentage = position.X / songValue.Width;
            double thumbPostion = thumb.Maximum * mousePositionPercentage;
            int second = (int)Math.Floor(thumbPostion / 1000);
            mainWindow?.MediaPlayer?.Seek(second);
        }

        private void openclose_setting(object sender, RoutedEventArgs e)
        {
            // Implement if you have a settings panel
        }

        private void skip_btn(object sender, RoutedEventArgs e)
        {
            mainWindow?.UIControl?.SkipBtn(sender, e);
        }

        private void stop_btn(object sender, RoutedEventArgs e)
        {
            mainWindow?.UIControl?.PauseResumeBtn(sender, e);
            stopresumeimg.Source = new BitmapImage(
                new Uri($"{Environment.CurrentDirectory}\\Images\\_Play.png"));
            (sender as Button).Click -= stop_btn;
            (sender as Button).Click += resume_btn;
        }

        private void resume_btn(object sender, RoutedEventArgs e)
        {
            mainWindow?.UIControl?.PauseResumeBtn(sender, e);
            stopresumeimg.Source = new BitmapImage(
                new Uri($"{Environment.CurrentDirectory}\\Images\\_Pause.png"));
            (sender as Button).Click -= resume_btn;
            (sender as Button).Click += stop_btn;
        }

        private void spectrum_btn(object sender, RoutedEventArgs e)
        {
            mainWindow?.UIControl?.SpectrumBtn(sender, e);
        }

        private void lyrics_btn(object sender, RoutedEventArgs e)
        {
            isLyrics = !isLyrics;
            // Implement lyric display logic if needed
        }

        private void lyricsSync_btn(object sender, RoutedEventArgs e)
        {
            mainWindow?.UIControl?.SyncLyricsBtn(sender, e);
        }

        private void loop_btn(object sender, RoutedEventArgs e)
        {
            mainWindow?.UIControl?.LoopBtn(sender, e);
            if (mainWindow?.loopTxt?.Text == " on ")
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

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var fadeIn = (Storyboard)FindResource("FadeInStoryboard");
            fadeIn.Begin();
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var fadeOut = (Storyboard)FindResource("FadeOutStoryboard");
            fadeOut.Begin();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
          mainWindow.Close();   
        }

        private void openmainButton_Click(object sender, RoutedEventArgs e)
        {
            OpenMainWindow();
        }
    }
}
