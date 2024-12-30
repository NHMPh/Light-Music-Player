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
        String data = "4 ";
        System.IO.Ports.SerialPort serialPort;
        SpectrumVisualizer spectrumVisualizer;
        DispatcherTimer timer6 = new DispatcherTimer();
        DispatcherTimer timer7 = new DispatcherTimer();
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
            timer6.Interval = TimeSpan.FromMilliseconds(1);
            timer6.Tick += UpadateLed;
            timer6.Start();
            timer7.Interval = TimeSpan.FromMilliseconds(3);
            timer7.Tick += UpadateLed2;
           
        }

        private void UpadateLed2(object sender, EventArgs e)
        {
            serialPort.Write(data); //Send the data
            timer6.Start();
            timer7.Stop();
            Console.WriteLine(data);
        }
        private void UpadateLed(object sender, EventArgs e)
        {
            data = "4 ";

            //Build data string
            for (int i = 0; i < 15; i++)
            {
                data += $"{spectrumVisualizer.buffer[i]} ";
            }


            if (serialPort.BytesToWrite == 0) // Buffer ready to send
            {
                //Delay 3ms 
                timer7.Start(); //Run UpadateLed2
                timer6.Stop();

            }

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
            var startColorBrush = ledSpectrum.LedStrips[0].Leds[0].LedDisplay.Background as SolidColorBrush;
            var endColorBrush = ledSpectrum.LedStrips[0].Leds[19].LedDisplay.Background as SolidColorBrush;

            if (startColorBrush != null && endColorBrush != null)
            {
                Color startColor = startColorBrush.Color;
                Color endColor = endColorBrush.Color;

                for (int i = 0; i < 20; i++)
                {
                    byte r = (byte)(startColor.R + (endColor.R - startColor.R) * i / 19);
                    byte g = (byte)(startColor.G + (endColor.G - startColor.G) * i / 19);
                    byte b = (byte)(startColor.B + (endColor.B - startColor.B) * i / 19);

                    ledSpectrum.LedStrips[0].Leds[i].LedDisplay.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
                }
            }
        }

        uint RgbToUint32(byte red, byte green, byte blue)
        {
            return ((uint)red << 16) | ((uint)green << 8) | (uint)blue;
        }
        private void SendColorData(object sender, RoutedEventArgs e)
        {
            String colorData1 = "0 "; //Part 1
            String colorData2 = "1 "; //Part 2
            String colorData3 = "2 "; //Part 3
            String colorData4 = "3 "; //Part 4

            //Build color data string
            for (int i = 0; i < 20; i++)
            {
                var backgroundBrush = ledSpectrum.LedStrips[0].Leds[i].LedDisplay.Background as SolidColorBrush;
                if (backgroundBrush != null)
                {
                    Color color = backgroundBrush.Color;
                    if (i < 5)
                    {
                        colorData1 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }
                    else if (i < 10)
                    {
                        colorData2 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }
                    else if (i < 15)
                    {
                        colorData3 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }
                    else
                    {
                        colorData4 += RgbToUint32(color.R, color.G, color.B) + " ";
                    }

                }
            }
            //Send 4 times
            timer6.Stop();
            Thread.Sleep(500);
            serialPort.Write(colorData1);
       
            Thread.Sleep(500);
            serialPort.Write(colorData2);
        
             Thread.Sleep(500);
             serialPort.Write(colorData3);
      
            Thread.Sleep(500);
            serialPort.Write(colorData4);
        
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
