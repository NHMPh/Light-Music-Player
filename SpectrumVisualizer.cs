using Accord.Math;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using NAudio.Dsp;
using Complex = NAudio.Dsp.Complex;
using PuppeteerSharp;
using System.Windows.Threading;

namespace NHMPh_music_player
{
    public class SpectrumVisualizer
    {

        const int numBars = 512;
        const int thresholdIndex = 50;
        const float multiplierLow = 1;//2000;
        const float multiplierHigh = 1;//5000;
        const float decreaseRateFactor = 1.2f;
        float[] multipliers = new float[numBars];

        MainWindow mainWindow;
     //   MediaPlayer mediaPlayer;
        List<ProgressBar> spectrumBars = new List<ProgressBar>();
        public double[] fbands = new double[2048];
        float[] decreaserate = new float[512];
        //15
        public int[] buffer = new int[16];
        double[] heightestBand = new double[16];
        public SpectrumVisualizer(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            CreateSpectrumBar();
            SetSpectrumBar();
            // Precalculate multiplier for each bar

            for (int i = 0; i < numBars; i++)
            {
                multipliers[i] = i < thresholdIndex ? multiplierLow : multiplierHigh;
            }
            for (int i = 0; i < 16; i++)
            {
                heightestBand[i] = 1;
            }
        }
        public void UpadateSpectrumBar15()
        {

            for (int i = 0; i < 8; i++)
            {
                double average = 0;
                for (int j = 0; j <= Math.Pow(2, i); j++)
                {
           //         average += 60 + mainWindow.DynamicVisualUpdate.Visualizer.fbands[(int)(2 * (Math.Pow(2, i) - 1 + j))];
                }
                average /= Math.Pow(2, i);
                if (heightestBand[2 * i] < average)
                {
                    heightestBand[2 * i] = average;
                }
                var spectrumValue = (average / heightestBand[2 * i]) * 19;
                spectrumValue = Math.Ceiling(spectrumValue);
                if (spectrumValue < 0) spectrumValue = 0;
                if (spectrumValue > 19) spectrumValue = 19;
                buffer[2 * i] = (int)spectrumValue;
                average = 0;
                for (int j = 0; j < Math.Pow(2, i); j++)
                {
                    average += 60+fbands[(int)(2 * (Math.Pow(2, i) - 1 + j)) + 1];
                }
                if (heightestBand[2 * i + 1] < average)
                {
                    heightestBand[2 * i + 1] = average;
                }
                spectrumValue = (average / heightestBand[2 * i + 1]) * 19;
                spectrumValue = Math.Ceiling(spectrumValue);
                if (spectrumValue < 0) spectrumValue = 0;
                if (spectrumValue > 19) spectrumValue = 19;
                buffer[2 * i + 1] = (int)spectrumValue;
            }
        }
        public void UpdateGraph(double[] magnitude)
        {
            fbands = magnitude;         
        }
        private List<ProgressBar> GetExistingProgressBars()
        {
            List<ProgressBar> existingProgressBars = new List<ProgressBar>();

            foreach (var child in mainWindow.spectrum_ctn.Children)
            {
                if (child is ProgressBar progressBar)
                {
                    existingProgressBars.Add(progressBar);
                }
            }
            List<ProgressBar> niceProgressBars = new List<ProgressBar>();

            for (int i = 0; i < 64; i++)
            {
                niceProgressBars.Add(existingProgressBars[64 + i]);
                niceProgressBars.Add(existingProgressBars[64 - i]);

            }
            niceProgressBars.RemoveAt(0);
            niceProgressBars.Add(existingProgressBars[0]);
            // return existingProgressBars;
            return niceProgressBars;
        }
        private void SetSpectrumBar()
        {
            spectrumBars = GetExistingProgressBars();
        }
        public void DrawGraph()
        {

            int positiveThreshhold = 70;
            for (int i = 0, j = 0; i <256; i++)
            {
                float multiplier = multipliers[i];

                if ((fbands[i] + positiveThreshhold)/ positiveThreshhold > spectrumBars[i].Value)
                {
                    int nextIndex = i + j;
                   // spectrumBars[i].Value = (((fbands[i] + positiveThreshhold))/60)*1.5f; // (fbands[i]) * multiplier; //((fbands[nextIndex] + fbands[nextIndex + 1]) / 2) * multiplier;
                    spectrumBars[i].Value = (((fbands[nextIndex] + positiveThreshhold + fbands[nextIndex + 1] + positiveThreshhold) / 2)/ positiveThreshhold) *1.5f * multiplier ;
                    j++;
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
        private void CreateSpectrumBar()
        {
            mainWindow.spectrum_ctn.Children.Clear();
            for (int i = 0; i < 128; i++)
            {
               
                ProgressBar progressBar = new ProgressBar()
                {
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = new SolidColorBrush(Colors.Cyan),
                    Width = 2,
                    Margin = new Thickness(0, 0, 1, 0),
                    Height = 60,
                    Maximum = 1,
                    Orientation = Orientation.Vertical,
                    Value = 0,


                };

                mainWindow.spectrum_ctn.Children.Add(progressBar);

            }



        }
      /*  private double[] FFT(double[] data)
        {
            int fftLength = data.Length;
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[fftLength];
            for (int i = 0; i < fftLength; i++)
            {
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            }
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            //    FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), fftComplex);

            // Calculate the magnitude spectrum and only return the first half
            double[] magnitude = new double[fftLength];
            for (int i = 0; i < fftLength; i++)
            {
                magnitude[i] = fftComplex[i].Magnitude;//Math.Sqrt(Math.Pow(fftComplex[i].X, 2) + Math.Pow(fftComplex[i].Y, 2));
                if (magnitude[i] != 0)
                {
                    magnitude[i] = 20 * Math.Log10((double)magnitude[i]);
                    //   if (magnitude[i] < -60)
                    //    magnitude[i] = -60;

                }
                else
                {
                    magnitude[i] = -60;
                }
                //Console.Write();





            }

            return magnitude;

        }*/
      /*  private double[] GetFFTdata()
        {

            int desireSamples =1024;
            double[] doublesData = new double[desireSamples];
            byte[] buffer = new byte[desireSamples * 4];
            int bytesRead;
            long originalPosition = mediaPlayer.Wave.Position;
            bytesRead = mediaPlayer.Wave.Read(buffer, 0, buffer.Length);
          
            mediaPlayer.Wave.Position = originalPosition;
            for (int i = 0; i < bytesRead / 4; i++)
            {
                doublesData[i] = BitConverter.ToSingle(buffer, i * 4);

            }

            // If bytesRead is less than desireSamples * 4, fill the rest of doublesData with zeros
            if (bytesRead < buffer.Length)
            {
                for (int i = bytesRead / 4; i < desireSamples; i++)
                {
                    doublesData[i] = 0;
                }
            }

            return FFT(doublesData);
        }*/
    }
}
