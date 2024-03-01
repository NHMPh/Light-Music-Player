using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            timer.Start();
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
                        if (i < 10) mutipler = 600;

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
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return ;
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
                filter.Color =Color.FromArgb(byte.Parse(filter_mod.Text), filter.Color.R, filter.Color.G, filter.Color.B);
            }
            catch
            {
                MessageBox.Show("Invaild input");
                return;
            }
                     
        }
        private void change_spectrum_width(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var bar in spectrumBars)
                {
                    bar.Width = float.Parse(width_mod.Text);
                   
                }
            }
            catch
            {
                MessageBox.Show("Invaild input");
                return;
            }

        }
        private void change_spectrum_height(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var bar in spectrumBars)
                {
                  
                    bar.Height = float.Parse(height_mod.Text);
                  
                }
            }
            catch
            {
                MessageBox.Show("Invaild input");
                return;
            }

        }
        private void change_spectrum_spaceing(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var bar in spectrumBars)
                {
                 
                    bar.Margin = new Thickness(0, 0, float.Parse(space_mod.Text), 0);
                }
            }
            catch
            {
                MessageBox.Show("Invaild input");
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
                    Foreground = new SolidColorBrush(Colors.Red),
                    Width = 2,
                    Margin = new Thickness(0, 0, 6, 0),
                    Height = 700,
                    Maximum = 1000,
                    Orientation = Orientation.Vertical,
                    Value = 0,
                    
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
