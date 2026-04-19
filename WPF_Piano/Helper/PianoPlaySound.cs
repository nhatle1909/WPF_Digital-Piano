using NAudio.CoreAudioApi;
using NAudio.Midi;
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
        //public double CalculateSongDuration(MidiFile midiFile)
        //{
        //    int ticksPerQuarterNote = midiFile.DeltaTicksPerQuarterNote;
        //    long maxTick = 0;
        //    foreach (var track in midiFile.Events)
        //    {
        //        if (track.Count > 0)
        //        {
        //            long lastEventTick = track.Max(e => e.AbsoluteTime);
        //            if (lastEventTick > maxTick) maxTick = lastEventTick;
        //        }
        //    }
          
     


        //    double bpm = 120.0;
        //    foreach (var track in midiFile.Events)
        //    {
        //        var tempoEvent = track.OfType<TempoEvent>().FirstOrDefault();
        //        if (tempoEvent != null)
        //        {
        //            bpm = tempoEvent.Tempo;
        //            break;
        //        }
        //    }
        //    double ticksPerSecond = (ticksPerQuarterNote * bpm) / 60.0;

        //    return (double)maxTick / ticksPerSecond;
        //}
        public int CalculateSongDuration(MidiFile midiFile)
        {
          
            int ticksPerQuarterNote = midiFile.DeltaTicksPerQuarterNote;

            double totalSeconds = 0;
            double currentBpm = 120;
            int lastTick = 0;

            var events = midiFile.Events.SelectMany(track => track)
                                        .OrderBy(e => e.AbsoluteTime);

            foreach (var midiEvent in events)
            {
                int deltaTicks = (int)midiEvent.AbsoluteTime - lastTick;
                if (deltaTicks > 0)
                {
                    totalSeconds += (double)deltaTicks / ticksPerQuarterNote * (60.0 / currentBpm);
                }

                lastTick = (int)midiEvent.AbsoluteTime;

                if (midiEvent is TempoEvent tempoEvent)
                {
                    currentBpm = tempoEvent.Tempo;
                }
            }

            // Convert double seconds to an integer of your choice
            return (int)Math.Round(totalSeconds ); // Returns Total Milliseconds
        }
        public string GetNoteName(int noteNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (noteNumber / 12) - 1;
            string name = noteNames[noteNumber % 12];
            return $"{name}{octave}";
        }
        public List<TempoEvent> GetTempoEvents(MidiFile midiFile)
        {
            List<TempoEvent> tempoEvents = new List<TempoEvent>();
            foreach (var track in midiFile.Events)
            {
                tempoEvents.AddRange(track.OfType<TempoEvent>());
            }
            return tempoEvents.OrderBy(e => e.AbsoluteTime).ToList();
        }
    }

}
