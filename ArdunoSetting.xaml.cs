using Accord.Math;
using NHMPh_music_player.LedStripCom;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NHMPh_music_player
{
    /// <summary>
    /// Interaction logic for ArdunoSetting.xaml
    /// </summary>
    public partial class ArdunoSetting : Window
    {
        LedSpectrum ledSpectrum;

        System.IO.Ports.SerialPort serialPort;
        SpectrumVisualizer spectrumVisualizer;
        DispatcherTimer timer6 = new DispatcherTimer();

        public ArdunoSetting( SpectrumVisualizer spectrumVisualizer)
        {
            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();
            SerialPort port = new SerialPort(ports[0]);
          //  MessageBox.Show(port.PortName.ToString());
            serialPort = new SerialPort(port.PortName, 115200);
            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            ledSpectrum = new LedSpectrum(1, 20, 30, 19, 0);
            this.MouseDown += Window_MouseDown;
            Spectrum_ctn.Children.Add(ledSpectrum.LedStripContainer);
            for (int i = 0; i < 19; i++)
            {
                ledSpectrum.LedStrips[0].Leds[i].LedDisplay.Click += LedDisplay_Click;
            }
            ledSpectrum.LedStrips[0].Leds[19].LedDisplay.Click += LedDisplay_ClickFull; ;
            this.spectrumVisualizer = spectrumVisualizer;
            timer6.Interval = TimeSpan.FromMilliseconds(20);
            timer6.Tick += UpadateLed;
            timer6.Start();
        }

        private void LedDisplay_ClickFull(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int i =0 ; i < 20; i++)
                {
                    ledSpectrum.LedStrips[0].Leds[i].LedDisplay.Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                }
    

            }
        }

        private void UpadateLed(object sender, EventArgs e)
        {


            String data = "4 ";
            for (int i = 0; i < 15; i++)
            {
                data += $"{spectrumVisualizer.buffer[i]} ";
            }



            serialPort.Write(data);
            //ledSpectrum.DrawSpectrum(buffer, snow);

        }

        private void LedDisplay_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                (sender as Button).Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));

            }

        }

        private void SetColorData(object sender, RoutedEventArgs e)
        {
            byte[] red = redTxt.Text.Split(' ').ToByte();
            byte[] green = greenTxt.Text.Split(' ').ToByte();
            byte[] blue = blueTxt.Text.Split(' ').ToByte();
            for (int i = 0; i < 20; i++)
            {
                Color color = Color.FromRgb(red[i], green[i], blue[i]);
                ledSpectrum.LedStrips[0].Leds[i].LedDisplay.Background = new SolidColorBrush(color);
            }
        }

        uint RgbToUint32(byte red, byte green, byte blue)
        {
            return ((uint)red << 16) | ((uint)green << 8) | (uint)blue;
        }
        private void SendColorData(object sender, RoutedEventArgs e)
        {
            String colorData1 = "0 ";
            String colorData2 = "1 ";
            String colorData3 = "2 ";
            String colorData4 = "3 ";




            for (int i = 0; i < 20; i++)
            {
                var backgroundBrush = ledSpectrum.LedStrips[0].Leds[i].LedDisplay.Background as SolidColorBrush;
                if (backgroundBrush != null)
                {
                    Color color = backgroundBrush.Color;
                    if (i < 5)
                    {
                        colorData1 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }else if (i < 10)
                    {
                        colorData2 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }else if(i < 15)
                    {
                        colorData3 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }
                    else
                    {
                        colorData4 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }
                   
                }
            }

         //   MessageBox.Show(colorData);
            timer6.Stop();
            Thread.Sleep(500);
            serialPort.Write(colorData1);
         //   MessageBox.Show(colorData1);
            Thread.Sleep(500);
            serialPort.Write(colorData2);
           // MessageBox.Show(colorData2);
             Thread.Sleep(500);
             serialPort.Write(colorData3);
           // MessageBox.Show(colorData3);
            Thread.Sleep(500);
            serialPort.Write(colorData4);
            // MessageBox.Show(colorData4);
            Thread.Sleep(500);
            timer6.Start();

        }
        private void SendDelayData(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                (sender as Button).Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void RefreshColor()
        {
        }
    }
}
