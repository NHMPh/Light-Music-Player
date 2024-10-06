using NAudio.CoreAudioApi.Interfaces;
using NAudio.MediaFoundation;
using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace NHMPh_music_player
{
    /// <summary>
    /// Class for reading any file that Media Foundation can play
    /// Will only work in Windows Vista and above
    /// Automatically converts to PCM
    /// If it is a video file with multiple audio streams, it will pick out the first audio stream
    /// </summary>
    public class _MediaFoundationReader : WaveStream
    {
        //
        // Summary:
        //     Allows customisation of this reader class
        public class MediaFoundationReaderSettings
        {
            //
            // Summary:
            //     Allows us to request IEEE float output (n.b. no guarantee this will be accepted)
            public bool RequestFloatOutput { get; set; }

            //
            // Summary:
            //     If true, the reader object created in the constructor is used in Read Should
            //     only be set to true if you are working entirely on an STA thread, or entirely
            //     with MTA threads.
            public bool SingleReaderObject { get; set; }

            //
            // Summary:
            //     If true, the reposition does not happen immediately, but waits until the next
            //     call to read to be processed.
            public bool RepositionInRead { get; set; }

            //
            // Summary:
            //     Sets up the default settings for MediaFoundationReader
            public MediaFoundationReaderSettings()
            {
                RepositionInRead = true;
            }
        }

        private WaveFormat waveFormat;

        private long length;

        private MediaFoundationReaderSettings settings;

        private readonly string file;

        private IMFSourceReader pReader;

        private long position;

        private byte[] decoderOutputBuffer;

        private int decoderOutputOffset;

        private int decoderOutputCount;

        private long repositionTo = -1L;

        //
        // Summary:
        //     WaveFormat of this stream (n.b. this is after converting to PCM)
        public override WaveFormat WaveFormat => waveFormat;

        //
        // Summary:
        //     The bytesRequired of this stream in bytes (n.b may not be accurate)
        public override long Length => length;

        //
        // Summary:
        //     Current position within this stream
        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Position cannot be less than 0");
                }

                if (settings.RepositionInRead)
                {
                    repositionTo = value;
                    position = value;
                }
                else
                {
                    Reposition(value);
                }
            }
        }

        //
        // Summary:
        //     WaveFormat has changed
        public event EventHandler WaveFormatChanged;

        //
        // Summary:
        //     Default constructor
        protected _MediaFoundationReader()
        {
        }

        //
        // Summary:
        //     Creates a new MediaFoundationReader based on the supplied file
        //
        // Parameters:
        //   file:
        //     Filename (can also be a URL e.g. http:// mms:// file://)
        public _MediaFoundationReader(string file)
            : this(file, null)
        {
        }

        //
        // Summary:
        //     Creates a new MediaFoundationReader based on the supplied file
        //
        // Parameters:
        //   file:
        //     Filename
        //
        //   settings:
        //     Advanced settings
        public _MediaFoundationReader(string file, MediaFoundationReaderSettings settings)
        {
            this.file = file;
            Init(settings);
        }

        //
        // Summary:
        //     Initializes
        protected void Init(MediaFoundationReaderSettings initialSettings)
        {
            MediaFoundationApi.Startup();
            settings = initialSettings ?? new MediaFoundationReaderSettings();
            IMFSourceReader iMFSourceReader = CreateReader(settings);
            waveFormat = GetCurrentWaveFormat(iMFSourceReader);
            iMFSourceReader.SetStreamSelection(-3, pSelected: true);
            length = GetLength(iMFSourceReader);
            if (settings.SingleReaderObject)
            {
                pReader = iMFSourceReader;
            }
            else
            {
                Marshal.ReleaseComObject(iMFSourceReader);
            }
        }

        private WaveFormat GetCurrentWaveFormat(IMFSourceReader reader)
        {
            reader.GetCurrentMediaType(-3, out var ppMediaType);
            MediaType mediaType = new MediaType(ppMediaType);
            _ = mediaType.MajorType;
            Guid subType = mediaType.SubType;
            int channelCount = mediaType.ChannelCount;
            int bitsPerSample = mediaType.BitsPerSample;
            int sampleRate = mediaType.SampleRate;
            if (subType == AudioSubtypes.MFAudioFormat_PCM)
            {
                return new WaveFormat(sampleRate, bitsPerSample, channelCount);
            }

            if (subType == AudioSubtypes.MFAudioFormat_Float)
            {
                return WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount);
            }

            string text = FieldDescriptionHelper.Describe(typeof(AudioSubtypes), subType);
            throw new InvalidDataException("Unsupported audio sub Type " + text);
        }

        private static MediaType GetCurrentMediaType(IMFSourceReader reader)
        {
            reader.GetCurrentMediaType(-3, out var ppMediaType);
            return new MediaType(ppMediaType);
        }

        //
        // Summary:
        //     Creates the reader (overridable by )
        protected virtual IMFSourceReader CreateReader(MediaFoundationReaderSettings settings)
        {
            MediaFoundationInterop.MFCreateSourceReaderFromURL(file, null, out var ppSourceReader);
            ppSourceReader.SetStreamSelection(-2, pSelected: false);
            ppSourceReader.SetStreamSelection(-3, pSelected: true);
            MediaType mediaType = new MediaType();
            mediaType.MajorType = MediaTypes.MFMediaType_Audio;
            mediaType.SubType = (settings.RequestFloatOutput ? AudioSubtypes.MFAudioFormat_Float : AudioSubtypes.MFAudioFormat_PCM);
            MediaType currentMediaType = GetCurrentMediaType(ppSourceReader);
            mediaType.ChannelCount = currentMediaType.ChannelCount;
            mediaType.SampleRate = currentMediaType.SampleRate;
            try
            {
                ppSourceReader.SetCurrentMediaType(-3, IntPtr.Zero, mediaType.MediaFoundationObject);
            }
            catch (COMException exception) when (exception.GetHResult() == -1072875852)
            {
                if (!(currentMediaType.SubType == AudioSubtypes.MFAudioFormat_AAC) || currentMediaType.ChannelCount != 1)
                {
                    throw;
                }

                mediaType.SampleRate = (currentMediaType.SampleRate *= 2);
                mediaType.ChannelCount = (currentMediaType.ChannelCount *= 2);
                ppSourceReader.SetCurrentMediaType(-3, IntPtr.Zero, mediaType.MediaFoundationObject);
            }

            Marshal.ReleaseComObject(currentMediaType.MediaFoundationObject);
            return ppSourceReader;
        }

        private long GetLength(IMFSourceReader reader)
        {
            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PropVariant>());
            try
            {
                int presentationAttribute = reader.GetPresentationAttribute(-1, MediaFoundationAttributes.MF_PD_DURATION, intPtr);
                switch (presentationAttribute)
                {
                    case -1072875802:
                        return 0L;
                    default:
                        Marshal.ThrowExceptionForHR(presentationAttribute);
                        break;
                    case 0:
                        break;
                }

                return (long)Marshal.PtrToStructure<PropVariant>(intPtr).Value * waveFormat.AverageBytesPerSecond / 10000000;
            }
            finally
            {
                PropVariant.Clear(intPtr);
                Marshal.FreeHGlobal(intPtr);
            }
        }

        private void EnsureBuffer(int bytesRequired)
        {
            if (decoderOutputBuffer == null || decoderOutputBuffer.Length < bytesRequired)
            {
                decoderOutputBuffer = new byte[bytesRequired];
            }
        }

        //
        // Summary:
        //     Reads from this wave stream
        //
        // Parameters:
        //   buffer:
        //     Buffer to read into
        //
        //   offset:
        //     Offset in buffer
        //
        //   count:
        //     Bytes required
        //
        // Returns:
        //     Number of bytes read; 0 indicates end of stream
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (pReader == null)
            {
                pReader = CreateReader(settings);
            }
            if (repositionTo != -1)
            {
                Reposition(repositionTo);
            }

            int bytesWritten = 0;
            // read in any leftovers from last time
            if (decoderOutputCount > 0)
            {
                bytesWritten += ReadFromDecoderBuffer(buffer, offset, count - bytesWritten);
            }

            while (bytesWritten < count)
            {
                IMFSample pSample;
                MF_SOURCE_READER_FLAG dwFlags;
                ulong timestamp;
                int actualStreamIndex;
                pReader.ReadSample(MediaFoundationInterop.MF_SOURCE_READER_FIRST_AUDIO_STREAM, 0, out actualStreamIndex, out dwFlags, out timestamp, out pSample);
                if ((dwFlags & MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_ENDOFSTREAM) != 0)
                {
                    // reached the end of the stream
                    break;
                }
                else if ((dwFlags & MF_SOURCE_READER_FLAG.MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED) != 0)
                {
                    waveFormat = GetCurrentWaveFormat(pReader);
                    OnWaveFormatChanged();
                    // carry on, but user must handle the change of format
                }
                else if (dwFlags != 0)
                {
                    throw new InvalidOperationException(String.Format("MediaFoundationReadError {0}", dwFlags));
                }

                IMFMediaBuffer pBuffer;
                pSample.ConvertToContiguousBuffer(out pBuffer);
                IntPtr pAudioData;
                int cbBuffer;
                int pcbMaxLength;
                pBuffer.Lock(out pAudioData, out pcbMaxLength, out cbBuffer);
                EnsureBuffer(cbBuffer);
                Marshal.Copy(pAudioData, decoderOutputBuffer, 0, cbBuffer);
                decoderOutputOffset = 0;
                decoderOutputCount = cbBuffer;

                long decoderPosition = (long)((timestamp * (ulong)waveFormat.AverageBytesPerSecond) / 10000000UL);
                if (decoderPosition + decoderOutputCount >= position)
                {
                    if (decoderPosition < position)
                    {
                        decoderOutputOffset = (int)(position + bytesWritten - decoderPosition);
                        // Align position to prevent loud noise.
                        decoderOutputOffset -= decoderOutputOffset % BlockAlign;

                        decoderOutputCount -= decoderOutputOffset;
                        // Clamp decoderOutputCount to be >= 0 to prevent length ArgumentExeption when reading.
                        decoderOutputCount = Math.Max(0, decoderOutputCount);
                    }

                    bytesWritten += ReadFromDecoderBuffer(buffer, offset + bytesWritten, count - bytesWritten);
                }


                pBuffer.Unlock();
                Marshal.ReleaseComObject(pBuffer);
                Marshal.ReleaseComObject(pSample);
            }
            position += bytesWritten;
            return bytesWritten;
        }

        private int ReadFromDecoderBuffer(byte[] buffer, int offset, int needed)
        {
            int num = Math.Min(needed, decoderOutputCount);
            Array.Copy(decoderOutputBuffer, decoderOutputOffset, buffer, offset, num);
            decoderOutputOffset += num;
            decoderOutputCount -= num;
            if (decoderOutputCount == 0)
            {
                decoderOutputOffset = 0;
            }

            return num;
        }

        private void Reposition(long desiredPosition)
        {
            PropVariant structure = PropVariant.FromLong(10000000 * repositionTo / waveFormat.AverageBytesPerSecond);
            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            try
            {
                Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
                pReader.SetCurrentPosition(Guid.Empty, intPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }

            decoderOutputCount = 0;
            decoderOutputOffset = 0;
            position = desiredPosition;
            repositionTo = -1L;
        }

        //
        // Summary:
        //     Cleans up after finishing with this reader
        //
        // Parameters:
        //   disposing:
        //     true if called from Dispose
        protected override void Dispose(bool disposing)
        {
            if (pReader != null)
            {
                Marshal.ReleaseComObject(pReader);
                pReader = null;
            }

            base.Dispose(disposing);
        }

        private void OnWaveFormatChanged()
        {
            this.WaveFormatChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
