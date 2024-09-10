using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Windows;

namespace NHMPh_music_player
{
    public class FFTSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int fftLength = 2048; // Must be a power of 2
        private readonly Complex[] fftBuffer;
        private float[] sampleBuffer;
        private double[] magnitude;
        private int bufferPos;

        private SpectrumVisualizer visualizer;
        public WaveFormat WaveFormat => source.WaveFormat;

        public FFTSampleProvider(ISampleProvider source, SpectrumVisualizer visualizer)
        {
            this.source = source;
            fftBuffer = new Complex[fftLength];
            sampleBuffer = new float[fftLength];
            magnitude = new double[fftLength];
            bufferPos = 0;
            this.visualizer = visualizer;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = source.Read(buffer, offset, count);



            for (int i = 0; i < samplesRead; i++)
            {
                sampleBuffer[bufferPos++] = buffer[offset + i];

                if (bufferPos >= fftLength)
                {
                    // Perform FFT
                    for (int j = 0; j < fftLength; j++)
                    {
                        fftBuffer[j].X = (float)(sampleBuffer[j] * FastFourierTransform.BlackmannHarrisWindow(j, fftLength));
                        fftBuffer[j].Y = 0; // Imaginary part is zero for real inputs
                    }

                    FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), fftBuffer);
                    for (int j = 0; j < fftLength; j++)
                    {
                        magnitude[j] = Math.Sqrt(Math.Pow(fftBuffer[j].X, 2) + Math.Pow(fftBuffer[j].Y, 2));
                      
                        magnitude[j] = 20 * Math.Log10(magnitude[j]);
                      
                        /*  if (magnitude[j] > 80)
                          {
                              magnitude[j] = 80; // Represents -Infinity dB, or 0 in linear scale
                          }*/

                    }

                    // FFT data is now in fftBuffer. Process it as needed.

                    //  visualizer.UpdateGraph(magnitude);

                  


                    visualizer.UpdateGraph(magnitude);
                    bufferPos = 0; // Reset buffer position
                }
            }





            return samplesRead;
        }
    }
}
