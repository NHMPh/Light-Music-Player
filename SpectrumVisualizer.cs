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

namespace NHMPh_music_player
{
    internal class SpectrumVisualizer
    {
     
        const int numBars = 126;
        const int thresholdIndex = 50;
        const int multiplierLow = 2000;
        const int multiplierHigh = 5000;
        const float decreaseRateFactor = 1.3f;
        int[] multipliers = new int[numBars];

        MediaFoundationReader _mfSpectrum;
        WaveChannel32 waveSpectrum;
        MainWindow mainWindow;
        MediaPlayer mediaPlayer;
        List<ProgressBar> spectrumBars = new List<ProgressBar>();
        double[] fbands = new double[2048];
        float[] decreaserate = new float[126];
        public SpectrumVisualizer( MainWindow mainWindow, MediaPlayer mediaPlayer ) 
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
        public void UpdateGraph()
        {
            try
            {           
                while (waveSpectrum.Position <= mediaPlayer.Wave.Position)
                {
                    fbands = GetFFTdata();              
                }
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
                    spectrumBars[i].Value = ((fbands[nextIndex] + fbands[nextIndex + 1]) / 2) * multiplier;
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
        private void CreateSpectrumBar()
        {
            mainWindow.spectrum_ctn.Children.Clear();
            for (int i = 0; i < 126; i++)
            {

                ProgressBar progressBar = new ProgressBar()
                {
                    BorderThickness = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Transparent),
                    Foreground = new SolidColorBrush(Colors.Cyan),
                    Width = 2,
                    Margin = new Thickness(0, 0, 1, 0),
                    Height = 60,
                    Maximum = 1000,
                    Orientation = Orientation.Vertical,
                    Value = 0,

                    
                };
               
                mainWindow.spectrum_ctn.Children.Add(progressBar);
               
            }

           

        }
        private double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            Complex[] fftComplex = new Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                fftComplex[i] = new Complex(data[i], 0.0);
            }
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            for (int i = 0; i < data.Length; i++)
            {
                fft[i] = fftComplex[i].Magnitude;

            }
            return fft;
        }
        private double[] GetFFTdata()
        {
            int desireByte = (int)Math.Pow(2, 13);
            double[] doublesData = new double[desireByte / 4];
            doublesData.Clear();
            byte[] buffer = new byte[desireByte];
            int bytesRead;
            bytesRead = waveSpectrum.Read(buffer, 0, desireByte);         
            for (int i = 0; i < bytesRead / 4; i++)
            {
                doublesData[i] = BitConverter.ToSingle(buffer, i * 4) * 10;
            }
            return FFT(doublesData);

        }
    }
}
