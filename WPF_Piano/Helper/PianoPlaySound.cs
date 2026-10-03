using NAudio.CoreAudioApi;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Windows;
using WPF_Piano.Model;

namespace WPF_Piano.Helper
{
    public class PianoPlaySound
    {
        private MixingSampleProvider bufferProvider;
        private readonly WasapiOut output;
        private static PianoPlaySound _Instance;
        private PianoSynthesis synthesis;
        // Active synth voices for held notes (support multiple simultaneous voices per MIDI note)
        private readonly Dictionary<int, List<SynthVoice>> _activeVoices = new();
        private const int MAX_VOICES_PER_NOTE = 6;
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
            UpdateSynthesis(PianoSettings.Instance.GetPianoSynthesis());
            PianoSettings.Instance.SynthesisUpdated += () => UpdateSynthesis(PianoSettings.Instance.GetPianoSynthesis());
        }

        // Start a note and keep it sounding until StopNote is called
        public void StartNote(int midiNote)
        {
            lock (_activeVoices)
            {
                double freq = MidiNumberToFrequency(midiNote);
                var voice = new SynthVoice((float)freq, synthesis?.Volume ?? 80, 0.25f);
                if (!_activeVoices.TryGetValue(midiNote, out var list))
                {
                    list = new List<SynthVoice>();
                    _activeVoices[midiNote] = list;
                }

                // enforce per-note polyphony limit
                if (list.Count >= MAX_VOICES_PER_NOTE)
                {
                    // retire the oldest voice (soft stop)
                    var oldest = list[0];
                    list.RemoveAt(0);
                    oldest.NoteOff();
                    Task.Run(async () =>
                    {
                        int waitMs = (int)(oldest.ReleaseSeconds * 1000) + 50;
                        await Task.Delay(waitMs);
                        lock (_activeVoices)
                        {
                            try { bufferProvider.RemoveMixerInput(oldest); } catch { }
                        }
                    });
                }

                list.Add(voice);
                bufferProvider.AddMixerInput(voice);
            }
        }

        // Trigger release for a held note; voice will be removed after its release finishes
        public void StopNote(int midiNote)
        {
            SynthVoice? voice = null;
            lock (_activeVoices)
            {
                if (!_activeVoices.TryGetValue(midiNote, out var list) || list.Count == 0) return;
                // use LIFO: stop most recently started voice for this note
                int idx = list.Count - 1;
                voice = list[idx];
                list.RemoveAt(idx);
                if (list.Count == 0) _activeVoices.Remove(midiNote);
            }

            if (voice == null) return;
            voice.NoteOff();

            // Remove from mixer after estimated release time asynchronously
            Task.Run(async () =>
            {
                int waitMs = (int)(voice.ReleaseSeconds * 1000) + 50;
                await Task.Delay(waitMs);
                lock (_activeVoices)
                {
                    try { bufferProvider.RemoveMixerInput(voice); } catch { }
                }
            });
        }

        private static double MidiNumberToFrequency(int noteNumber)
        {
            return 440.0 * Math.Pow(2.0, (noteNumber - 69) / 12.0);
        }
        public void PlaySound(float frequency, int durationInMiliSeconds)
        {
            int totalSamples = sampleRate * durationInMiliSeconds / 1000;
         

            byte[] buffer = new byte[totalSamples * bytesPerSample];

            for (int i = 0; i < totalSamples; i++)
            {
                double time = (double)i / sampleRate;
                double envelope = Math.Exp(-decayRate * time);
                double sampleValue = (synthesis.Volume / 100.0) * Math.Sin(2 * Math.PI * frequency * time);

                sampleValue *= envelope;
                sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);

                short sample = (short)(sampleValue * short.MaxValue);
                buffer[i * bytesPerSample] = (byte)(sample & 0xFF);
                buffer[i * bytesPerSample + 1] = (byte)(sample >> 8 & 0xFF);
            }


            //// Extract slider settings (expected range: 0.0 to 1.0)
            //double attackTime = (double)synthesis.Attack;
            //double decayTime = (double)synthesis.Decay;
            //double sustainLevel = (double)synthesis.Sustain;
            //double volumeFactor = (double)synthesis.Volume / 100.0;

            //// Safety guard rails to prevent division by zero on zeroed sliders
            //if (attackTime <= 0) attackTime = 0.001;
            //if (decayTime <= 0) decayTime = 0.001;

            //for (int i = 0; i < totalSamples; i++)
            //{
            //    double time = (double)i / sampleRate;
            //    double envelope = 0.0;

            //    if (time < attackTime)
            //    {
            //        envelope = 1.0 - Math.Exp(-(5.0 / attackTime) * time);
            //    }
            //    else if (time < attackTime + decayTime)
            //    {
            //        double timeInDecay = time - attackTime;
            //        double decayFactor = Math.Exp(-(5.0 / decayTime) * timeInDecay);
            //        envelope = sustainLevel + (1.0 - sustainLevel) * decayFactor;
            //    }
            //    else
            //    {
            //        double sustainTime = time - (attackTime + decayTime);
            //        envelope = sustainLevel * Math.Exp(-0.4 * sustainTime);
            //    }

            //    double sampleValue = volumeFactor * Math.Sin(2 * Math.PI * frequency * time);

            //    sampleValue *= envelope;
            //    sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);

            //    short sample = (short)(sampleValue * short.MaxValue);

            //    buffer[i * bytesPerSample] = (byte)(sample & 0xFF);
            //    buffer[i * bytesPerSample + 1] = (byte)((sample >> 8) & 0xFF);
            //}
            bufferProvider.AddMixerInput(new RawSourceWaveStream(new MemoryStream(buffer), new WaveFormat(sampleRate, 16, 1)).ToSampleProvider());
        }

        // Simple synth voice providing sine wave with release envelope
        private class SynthVoice : ISampleProvider
        {
            private readonly int sampleRate = 44100;
            private readonly float frequency;
            private readonly float amplitude;
            private double phase;
            private volatile bool releasing = false;
            public readonly float ReleaseSeconds;
            private double releaseProgress = 0.0;

            public SynthVoice(float frequency, double volumePercent, float releaseSeconds)
            {
                this.frequency = frequency;
                this.amplitude = (float)((volumePercent / 100.0) * 0.5);
                this.ReleaseSeconds = releaseSeconds;
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            }

            public WaveFormat WaveFormat { get; }

            public void NoteOff()
            {
                releasing = true;
            }

            public int Read(float[] buffer, int offset, int count)
            {
                for (int n = 0; n < count; n++)
                {
                    double env = 1.0;
                    if (releasing)
                    {
                        releaseProgress += 1.0 / (ReleaseSeconds * sampleRate);
                        env = Math.Max(0.0, 1.0 - releaseProgress);
                    }

                    double sample = amplitude * Math.Sin(2 * Math.PI * frequency * phase / sampleRate) * env;
                    buffer[offset + n] = (float)sample;
                    phase += 1.0;
                    if (phase >= sampleRate) phase -= sampleRate;
                }
                return count;
            }
        }

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
            return (int)Math.Round(totalSeconds); // Returns Total Milliseconds
        }
        public string GetNoteName(int noteNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (noteNumber / 12) - 1;
            string name = noteNames[noteNumber % 12];
            return $"{name}{octave}";
        }
        private void UpdateSynthesis(PianoSynthesis synthesis)
        {
        
            if (synthesis != null)
            {
              this.synthesis = synthesis;
            }
        }
    }

}
