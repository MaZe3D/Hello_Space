using System.Diagnostics;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Dsp;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using NAudio.Wave.SampleProviders;


namespace Hello_Space
{
    public class Audio : IDisposable
    {
        AudioFileReader audioFileReader;
        public WaveOutEvent waveOut;

        public bool EnableSampleOutput = false;

        public double SampleRate => Convert.ToDouble(audioFileReader.WaveFormat.SampleRate);
        public double SecondsPerSample => 1.0d / SampleRate;
        public double Time => audioFileReader.CurrentTime.TotalSeconds;
        public double Length => audioFileReader.TotalTime.TotalSeconds;

        MeteringSampleProvider meteringSampleProvider;

        public StereoAudio defaultData = new StereoAudio();

        public StereoAudio bassData = new StereoAudio();
        public StereoAudio midData = new StereoAudio();
        public StereoAudio highData = new StereoAudio();

        public Audio(string path)
        {
            audioFileReader = new AudioFileReader(path);
            waveOut = new WaveOutEvent();
            waveOut.Init(audioFileReader);
            ExtractSamples();

            StartFromBeginning();
            bassData.Left = ApplyFilter(defaultData.Left, BiQuadFilter.LowPassFilter((float)SampleRate, 250, 1));
            bassData.Right = ApplyFilter(defaultData.Right, BiQuadFilter.LowPassFilter((float)SampleRate, 250, 1));

            StartFromBeginning();
            midData.Left = ApplyFilter(defaultData.Left, BiQuadFilter.BandPassFilterConstantSkirtGain((float)SampleRate, 1000, 1));
            midData.Right = ApplyFilter(defaultData.Right, BiQuadFilter.BandPassFilterConstantSkirtGain((float)SampleRate, 1000, 1));

            StartFromBeginning();
            highData.Left = ApplyFilter(defaultData.Left, BiQuadFilter.HighPassFilter((float)SampleRate, 4000, 1));
            highData.Right = ApplyFilter(defaultData.Right, BiQuadFilter.HighPassFilter((float)SampleRate, 4000, 1));


        }

        public void Dispose()
        {
            waveOut.Dispose();
            audioFileReader.Dispose();
        }

        // Extract the samples from AudioFileReader into the samplesLeft and samplesRight arrays
        void ExtractSamples()
        {
            // Reset the stream to the beginning
            StartFromBeginning();

            int sampleCount = (int)((audioFileReader.Length / sizeof(float) / audioFileReader.WaveFormat.Channels) + 1);
            Debug.WriteLine("Sample Count: " + sampleCount);

            meteringSampleProvider = new MeteringSampleProvider(audioFileReader);

            float[] allSamples = new float[sampleCount * audioFileReader.WaveFormat.Channels];

            audioFileReader.Read(allSamples, 0, sampleCount * audioFileReader.WaveFormat.Channels);

            if (audioFileReader.WaveFormat.Channels == 2)
            {
                defaultData.Left = new float[sampleCount];
                defaultData.Right = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    defaultData.Left[i] = allSamples[i * 2];
                    defaultData.Right[i] = allSamples[i * 2 + 1];
                }
            }
        }

        // Metering Sample Event Handler

        public StereoSample GetSampleAtTime(float time, AudioFrequencyBand band)
        {
            var sampleIndex = (int)(time / (float)SecondsPerSample);
            StereoAudio data = defaultData;

            switch (band)
            {
                case AudioFrequencyBand.Bass:
                    data = bassData;
                    break;
                case AudioFrequencyBand.Mid:
                    data = midData;
                    break;
                case AudioFrequencyBand.High:
                    data = highData;
                    break;
            }

            if (sampleIndex >= data.Left.Length)
            {
                sampleIndex = data.Left.Length - 1;
            }

            Debug.WriteLineIf(EnableSampleOutput, $"Sample Index: {sampleIndex}\t Time: {time}\t sampleLeft: {data.Left[sampleIndex]:0.000}\t sampleRight: {data.Right[sampleIndex]:0.000}");

            return new StereoSample
            {
                Left = data.Left[sampleIndex],
                Right = data.Right[sampleIndex]
            };
        }

        public StereoSample GetSampleAtTimeSpan(float offset, float time, AudioFrequencyBand band)
        {
            StereoAudio data = defaultData;
            switch (band)
            {
                case AudioFrequencyBand.Bass:
                    data = bassData;
                    break;
                case AudioFrequencyBand.Mid:
                    data = midData;
                    break;
                case AudioFrequencyBand.High:
                    data = highData;
                    break;
            }

            var samples = (int)(time / (float)SecondsPerSample);
            int startIndex = (int)(offset / (float)SecondsPerSample);
            int endIndex = startIndex + samples;

            // average the samples in the range
            float left = 0;
            float right = 0;

            if (endIndex >= data.Left.Length)
            {
                endIndex = data.Left.Length - 1;
            }

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                left += data.Left[i];
                right += data.Right[i];
            }

            var sample = new StereoSample
            {
                Left = left / samples,
                Right = right / samples
            };

            Debug.WriteLineIf(EnableSampleOutput, $"Sample Index: {samples}\t Time: {time}\t sampleLeft: {sample.Left:0.000}\t sampleRight: {data.Right:0.000}");

            return sample;
        }

        float[] ApplyFilter(float[] data, BiQuadFilter filter)
        {
            float[] filteredData = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                filteredData[i] = filter.Transform(data[i]);
            }

            return filteredData;
        }

        public void StartFromBeginning()
        {
            audioFileReader.Seek(0, SeekOrigin.Begin);
            waveOut.Init(audioFileReader);
        }
    }

    public struct StereoAudio
    {
        public float[] Left;
        public float[] Right;
    }

    public struct StereoSample
    {
        public float Left;
        public float Right;
    }

    public enum AudioFrequencyBand
    {
        Bass,
        Mid,
        High
    }
}