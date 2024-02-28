using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for FullscreenSpectrum.xaml
    /// </summary>
    public partial class FullscreenSpectrum : Window
    {
        private DispatcherTimer timer;
        float[] decreaserate = new float[512];
        List<ProgressBar> spectrumBars = new List<ProgressBar>();

        public FullscreenSpectrum()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the interval as needed
            timer.Tick += async (sender, e) =>
            {
                await UpadateSpectrum();
            };
            InitializeComponent();
            CreateSpectrumBar();
            this.MouseDown += Window_MouseDown;
            this.MouseMove += FullscreenSpectrum_MouseMove;          
            this.WindowState = WindowState.Maximized;
            timer.Start();
        }

        private void FullscreenSpectrum_MouseMove(object sender, MouseEventArgs e)
        {
            setting.Opacity = 1;
            this.MouseMove -= FullscreenSpectrum_MouseMove;
        }

        private async Task UpadateSpectrum()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0, j = 0; i < 256; i++)
                    {
                        int mutipler = 5000;
                        if (i < 50) mutipler = 2000;


                        if (MainWindow.fbands[i] * mutipler > spectrumBars[i].Value)
                        {


                            spectrumBars[i].Value = ((MainWindow.fbands[i + j] + MainWindow.fbands[i + j + 1]) / 2) * mutipler;
                            j++;



                            decreaserate[i] = 10f;
                        }
                        else
                        {
                            spectrumBars[i].Value -= 1 * decreaserate[i];
                            decreaserate[i] *= 1.3f;
                        }
                    }
                });
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                setting.Opacity = 0;
            this.MouseMove += FullscreenSpectrum_MouseMove;


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
        private void CreateSpectrumBar()
        {
            for (int i = 0; i < 256; i++)
            {

                ProgressBar progressBar = new ProgressBar()
                {
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = new SolidColorBrush(Colors.Red),
                    Width = 7,
                    Margin = new Thickness(0, 0, 1, 0),
                    Height = 700,
                    Maximum = 1000,
                    Orientation = Orientation.Vertical,
                    Value = 0
                };
                /*  if(i==125) progressBar.Value = 1000;

                  spectrum_ctn.Children.Add(progressBar);*/
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
    }
}
