using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMPh_music_player
{
    public class AnalysisSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private float[] analysisBuffer;

        public WaveFormat WaveFormat => source.WaveFormat;

        public AnalysisSampleProvider(ISampleProvider source)
        {
            this.source = source;
            this.analysisBuffer = new float[1024]; // Adjust size as needed
        }

        public int Read(float[] buffer, int offset, int count)
        {
            Console.WriteLine("Running");
            // Read audio data from the source
            int samplesRead = source.Read(buffer, offset, count);

            // Copy data to analysis buffer
            Array.Copy(buffer, offset, analysisBuffer, 0, samplesRead);

            // Here you can analyze the data in analysisBuffer
            // For example, perform FFT, signal processing, etc.

            return samplesRead;
        }
    }
}
