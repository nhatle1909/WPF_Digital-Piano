using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace WPF_Piano.Helper
{
    public class PianoPlaySound
    {
        private MixingSampleProvider bufferProvider;
        private readonly WasapiOut output;
        private static PianoPlaySound _Instance;
        public static PianoPlaySound Instance
        {
            get
            {
                if (_Instance == null) _Instance = new PianoPlaySound();
                return _Instance;
            }
        }
        #region Sound settings
        double[] amplitudes = { 1.0, 0.3, 0.2, 0.1 };
        int sampleRate = 44100;
        int bytesPerSample = 2; // 16-bit audio
        double decayRate = 3; // Higher = faster damping
        #endregion
        private PianoPlaySound()
        {
            bufferProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 1)) { ReadFully = true };
            output = new WasapiOut(AudioClientShareMode.Shared, false, 20);
            output.Init(bufferProvider);
            output.Play();
        }
        public void PlaySound(float frequency, int durationInMiliSeconds)
        {
            int totalSamples = sampleRate * durationInMiliSeconds / 1000;


            byte[] buffer = new byte[totalSamples * bytesPerSample];

            for (int i = 0; i < totalSamples; i++)
            {
                double time = (double)i / sampleRate;
                double envelope = Math.Exp(-decayRate * time);
                double sampleValue = amplitudes[0] * Math.Sin(2 * Math.PI * frequency * time);
                //double sampleValue = 0.0;
                //for (int h = 1; h <= amplitudes.Length; h++)
                //{
                //    sampleValue += amplitudes[h - 1] * Math.Sin(2 * Math.PI * frequency * time);
                //}

                // Normalize to avoid clipping
                sampleValue *= envelope;
                sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);

                short sample = (short)(sampleValue * short.MaxValue);
                buffer[i * bytesPerSample] = (byte)(sample & 0xFF);
                buffer[i * bytesPerSample + 1] = (byte)(sample >> 8 & 0xFF);
            }

            // Create wave file and play
            bufferProvider.AddMixerInput(new RawSourceWaveStream(new MemoryStream(buffer), new WaveFormat(sampleRate, 16, 1)).ToSampleProvider());

        }

    }

}
