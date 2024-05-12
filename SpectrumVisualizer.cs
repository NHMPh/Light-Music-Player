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

namespace NHMPh_music_player
{
    public class SpectrumVisualizer
    {

        const int numBars = 512;
        const int thresholdIndex = 50;
        const int multiplierLow = 1;//2000;
        const int multiplierHigh = 1;//5000;
        const float decreaseRateFactor = 1.2f;
        int[] multipliers = new int[numBars];

        MediaFoundationReader _mfSpectrum;
        WaveChannel32 waveSpectrum;
        MainWindow mainWindow;
        MediaPlayer mediaPlayer;
        List<ProgressBar> spectrumBars = new List<ProgressBar>();
        public double[] fbands = new double[2048];
        float[] decreaserate = new float[512];
        //15
        public int[] buffer = new int[15];
        double[] heightestBand = new double[15];
        public SpectrumVisualizer(MainWindow mainWindow, MediaPlayer mediaPlayer)
        {

            this.mediaPlayer = mediaPlayer;
            this.mediaPlayer.OnPositionChange += SetSpectrumWave;
            this.mainWindow = mainWindow;
            CreateSpectrumBar();
            SetSpectrumBar();
            // Precalculate multiplier for each bar

            for (int i = 0; i < numBars; i++)
            {
                multipliers[i] = i < thresholdIndex ? multiplierLow : multiplierHigh;
            }
        }

        private void SetSpectrumWave(object sender, EventArgs e)
        {
            waveSpectrum.Position = mediaPlayer.Wave.Position;
        }

        public void SetSpectrumWave()
        {
            waveSpectrum = new WaveChannel32(new MediaFoundationReader(mediaPlayer.PlayBackUrl));

        }
        public void UpadateSpectrumBar15()
        {

            for (int i = 0; i < 15; i++)
            {
                double average = 0;
                for (int j = 0; j < 6; j++)
                {

                    average += fbands[i * 6 + j];
                }
                average /= 6;
                if (heightestBand[i] < average)
                {
                    heightestBand[i] = average;
                }
                if (average == 0) heightestBand[i] = 0;
                var spectrumValue = (average / heightestBand[i]) * 19;
                spectrumValue = (int)spectrumValue;
                if (spectrumValue >= buffer[i])
                {
                    buffer[i] = (int)spectrumValue;

                }
            }
        }
        public void UpdateGraph()
        {

            try
            {

                //   waveSpectrum.Position = mediaPlayer.Wave.Position;
                while (waveSpectrum.Position < mediaPlayer.Wave.Position)
                    fbands = GetFFTdata();
                //  waveSpectrum.Position = mediaPlayer.Wave.Position;
                // mediaPlayer.ResumeMusic();
            }
            catch { }

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

            for (int i = 0; i < 63; i++)
            {
                niceProgressBars.Add(existingProgressBars[63 + i]);
                niceProgressBars.Add(existingProgressBars[63 - i]);

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
            for (int i = 0, j = 0; i < numBars; i++)
            {
                int multiplier = multipliers[i];

                if (fbands[i] * multiplier > spectrumBars[i].Value)
                {
                    int nextIndex = i + j;
                    spectrumBars[i].Value = ((fbands[nextIndex] + fbands[nextIndex + 1]) / 2) * multiplier; // (fbands[i]) * multiplier; //((fbands[nextIndex] + fbands[nextIndex + 1]) / 2) * multiplier;
                    j++;
                    decreaserate[i] = 1f;
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
            for (int i = 0; i < numBars/2; i++)
            {

                ProgressBar progressBar = new ProgressBar()
                {
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = new SolidColorBrush(Colors.Cyan),
                    Width = 2,
                    Margin = new Thickness(0, 0, 1, 0),
                    Height = 60,
                    Maximum = 50,
                    Orientation = Orientation.Vertical,
                    Value = 0,


                };
              
                mainWindow.spectrum_ctn.Children.Add(progressBar);

            }



        }
        private double[] FFT(double[] data)
        {
            int fftLength = data.Length;
            Complex[] fftComplex = new Complex[fftLength];
            for (int i = 0; i < fftLength; i++)
            {
                fftComplex[i] = new Complex();
                fftComplex[i].X = (float)data[i];
            }
            FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), fftComplex);

            // Calculate the magnitude spectrum and only return the first half
            double[] magnitude = new double[fftLength];
            for (int i = 0; i < fftLength; i++)
            {
                magnitude[i] = Math.Sqrt(Math.Pow(fftComplex[i].X, 2) + Math.Pow(fftComplex[i].Y, 2));
                magnitude[i] = 20 * Math.Log10((double)magnitude[i]);
            }
            return magnitude;

        }
        private double[] GetFFTdata()
        {
            int desireSamples = 4096;
            double[] doublesData = new double[desireSamples];
            byte[] buffer = new byte[desireSamples * 4];
            int bytesRead;

            bytesRead = waveSpectrum.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < bytesRead / 4; i++)
            {
                doublesData[i] = BitConverter.ToSingle(buffer, i * 4) * 1000;
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
        }
    }
}
