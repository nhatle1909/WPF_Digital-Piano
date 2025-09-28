using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static WPF_Piano.PianoPlaySound;

namespace WPF_Piano
{
    public class PianoPlaySound 
    {
        private MixingSampleProvider bufferProvider;
        private readonly WasapiOut output;
        public PianoPlaySound()
        {
            bufferProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 1)) { ReadFully = true };
            output = new WasapiOut(AudioClientShareMode.Shared, false, 20);
            output.Init(bufferProvider);
            output.Play();
        }
        public void PlaySound( float frequency, int durationInMiliSeconds, int sampleRate = 44100)
        {

            // Sound settings
            //double amplitude = 1; // Volume level (0.0 to 1.0)
            int bytesPerSample = 2; // 16-bit audio
            int totalSamples = (int)(sampleRate * durationInMiliSeconds / 1000);

            double[] amplitudes = { 1.0, 0.3, 0.2, 0.1 };
            byte[] buffer = new byte[totalSamples * bytesPerSample];
            double decayRate = 3; // Higher = faster damping
                                  // Generate the sound wave
            for (int i = 0; i < totalSamples; i++)
            {
                double time = (double)i / sampleRate;
                double envelope = Math.Exp(-decayRate * time);
                //double sampleValue = amplitudes[0] * Math.Sin(2 * Math.PI * frequency * time);
                double sampleValue = 0.0;
                for (int h = 1; h <= amplitudes.Length; h++)
                {
                    sampleValue += amplitudes[h - 1] * Math.Sin(2 * Math.PI * frequency * time);
                }

                // Normalize to avoid clipping
                sampleValue *= envelope;
                sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);

                short sample = (short)(sampleValue * short.MaxValue);
                buffer[i * bytesPerSample] = (byte)(sample & 0xFF);
                buffer[i * bytesPerSample + 1] = (byte)((sample >> 8) & 0xFF);
            }

            // Create wave file and play
            bufferProvider.AddMixerInput(new RawSourceWaveStream(new MemoryStream(buffer), new NAudio.Wave.WaveFormat(sampleRate, 16, 1)).ToSampleProvider());

        }
    
    }
    public  class NoteTileInfo
    {
        public string NoteName { get; set; }
        //public int NoteLength { get; set; }
        public MetricTimeSpan StartTime { get; set; }
        public int Velocity { get; set; }
        public MetricTimeSpan Duration { get; set; }
        public static List<NoteTileInfo> ExtractNoteInfo(MidiFile midiFile)
        {
            var noteTiles = new List<NoteTileInfo>();
            if (midiFile == null) return noteTiles;

            bool isSingleTrack = midiFile.Chunks.Count() == 1;
            TempoMap tempo = midiFile.GetTempoMap();
            var noteList = midiFile.GetTrackChunks().ToList()[isSingleTrack ? 0 : 1].GetNotes();
            foreach (var note in noteList)
            {
                var startTime = note.TimeAs<MetricTimeSpan>(tempo); // Convert to seconds
                var duration = note.LengthAs<MetricTimeSpan>(tempo); // Convert to seconds
                var octave = note.Octave.ToString();
                var noteLabel = note.NoteName.ToString().First();
                var noteTile = new NoteTileInfo
                {
                    NoteName = note.NoteName.ToString().Contains("Sharp") ? noteLabel + "#" + octave : noteLabel + octave,

                    StartTime = startTime,
                    Duration = duration,
                    Velocity = (int)(note.Length) / 10 // Scale velocity to fit in UI height (0-127 to 0-60)
                };
                noteTiles.Add(noteTile);
            }
            return noteTiles;
        }
    }
}
