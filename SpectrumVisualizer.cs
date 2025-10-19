using System;
using System.Collections.Generic;

using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using NAudio.Dsp;
using Complex = NAudio.Dsp.Complex;
using System.Windows.Threading;
using System.ComponentModel;

namespace NHMPh_music_player
{
    public class SpectrumVisualizer
    {

        const int numBars = 512;
        const int thresholdIndex = 50;
        const float multiplierLow = 1;//2000;
        const float multiplierHigh = 1;//5000;
        const float decreaseRateFactor = 1.30f;
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

        private double AverageCalculator(int startIndex, int endIndex)
        {
            double average = 0;
            int zeroCount = 0;
            int elements = endIndex - startIndex;
            for (int i = 0; i < elements; i++)
            {
                double value = 80 + mainWindow.DynamicVisualUpdate.Visualizer.fbands[startIndex + i];
                if (value < 0)
                {
                    value = 0;
                    zeroCount++;
                }
                average += value;
            }
            if (elements <= 0 || average <= 0) 
                return 0;
            return average / (elements - zeroCount);
        }

        public void UpadateSpectrumBar15(double[] magnitude)
        {
            for (int i = 0; i < 15; i++)
            {
                double average = 0;
                double spectrumValue = 0;
                switch (i)
                {
                    case 0:
                        average = AverageCalculator(0, 4);
                        break;
                    case 1:
                        average = AverageCalculator(4, 5);
                        break;
                    case 2:
                        average = AverageCalculator(5, 8);
                        break;
                    case 3:
                        average = AverageCalculator(8, 14);
                        break;
                    case 4:
                        average = AverageCalculator(14, 24);
                        break;
                    case 5:
                        average = AverageCalculator(24, 45);
                        break;
                    case 6:
                        average = AverageCalculator(45, 88);
                        break;
                    case 7:
                        average = AverageCalculator(88, 173);
                        break;
                    case 8:
                        average = AverageCalculator(173, 180);
                        break;
                    case 9:
                        average = AverageCalculator(180, 197);
                        break;
                    case 10:
                        average = AverageCalculator(197, 216);
                        break;
                    case 11:
                        average = AverageCalculator(216, 271);
                        break;
                    case 12:
                        average = AverageCalculator(271, 327);
                        break;
                    case 13:
                        average = AverageCalculator(327, 380);
                        break;
                    case 14:
                        average = AverageCalculator(380, 435);
                        break;
                }
                if (heightestBand[i] < average)
                {
                    heightestBand[i] = average;
                }
                if(i==0)
                spectrumValue = (average / 65) * 19;
                else
                    spectrumValue = (average / 50) * 19;
                spectrumValue = Math.Ceiling(spectrumValue);
                if (spectrumValue < 0) spectrumValue = 0;
                if (spectrumValue > 19) spectrumValue = 19;
                buffer[i] = (int)spectrumValue;
            }
        }

        public void UpdateGraph(double[] magnitude)
        {
            fbands = magnitude;
           /* if (!MusicSetting.isFullScreen)
                Application.Current.Dispatcher.Invoke(() => DrawGraph());*/
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
            //0.005f
            const float decreaseconst = 0.010f;
            int positiveThreshhold = 80;
            for (int i = 0, j = 0; i < 128; i++)
            {
                float multiplier = 1.0f;

                if ((fbands[i]+ positiveThreshhold) / positiveThreshhold > spectrumBars[i].Value)
                {
                    int nextIndex = i + j;
                    // spectrumBars[i].Value = (((fbands[i] + positiveThreshhold))/60)*1.5f; // (fbands[i]) * multiplier; //((fbands[nextIndex] + fbands[nextIndex + 1]) / 2) * multiplier;
                    spectrumBars[i].Value = (((fbands[nextIndex]  + fbands[nextIndex + 1] +2*positiveThreshhold) / 2) / positiveThreshhold) * 1.5f * multiplier;
                    //MessageBox.Show(fbands[i].ToString());
                    j++;
                    decreaserate[i] = decreaseconst;
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
        public void DecreaseGraph()
        {
           /* int positiveThreshhold = 80;
            for (int i = 0, j = 0; i < 128; i++)
            {
                if (spectrumBars[i].Value > 0 && (fbands[i] + positiveThreshhold) / positiveThreshhold < spectrumBars[i].Value)
                {
                    spectrumBars[i].Value -= 1 * decreaserate[i];
                    decreaserate[i] *= decreaseRateFactor;
                }
               
            }*/
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
