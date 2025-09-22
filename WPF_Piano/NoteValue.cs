using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Piano
{
    public static class NoteValue
    {
        // Static value include all 88 standard piano keys and their correspondingfrequencies
        
        public static readonly Dictionary<string,float> NoteFrequencies = new Dictionary<string,float>
        {
            { "C0", 16.35f }, { "C#0", 17.32f }, { "D0", 18.35f }, { "D#0", 19.45f }, { "E0", 20.60f }, { "F0", 21.83f }, { "F#0", 23.12f }, { "G0", 24.50f }, { "G#0", 25.96f }, { "A0", 27.50f }, { "A#0", 29.14f }, { "B0", 30.87f },
            { "C1", 32.70f }, { "C#1", 34.65f }, { "D1", 36.71f }, { "D#1", 38.89f }, { "E1", 41.20f }, { "F1", 43.65f }, { "F#1", 46.25f }, { "G1", 49.00f }, { "G#1", 51.91f }, { "A1", 55.00f }, { "A#1", 58.27f }, { "B1", 61.74f },
            { "C2", 65.41f }, { "C#2", 69.30f }, { "D2", 73.42f }, { "D#2", 77.78f }, { "E2", 82.41f }, { "F2", 87.31f }, { "F#2", 92.50f }, { "G2", 98.00f }, { "G#2", 103.83f }, { "A2", 110.00f }, { "A#2", 116.54f }, { "B2", 123.47f },
            { "C3", 130.81f }, { "C#3", 138.59f }, { "D3", 146.83f }, { "D#3", 155.56f }, { "E3", 164.81f }, { "F3", 174.61f }, { "F#3", 185.00f }, { "G3", 196.00f }, { "G#3", 207.65f }, { "A3", 220.00f }, { "A#3", 233.08f }, { "B3", 246.94f },
            { "C4", 261.63f }, { "C#4", 277.18f }, { "D4", 293.66f }, { "D#4", 311.13f }, { "E4", 329.63f }, { "F4", 349.23f }, { "F#4", 369.99f }, { "G4", 392.00f }, { "G#4", 415.30f }, { "A4", 440.00f }, { "A#4", 466.16f }, { "B4", 493.88f },
            { "C5", 523.25f }, { "C#5", 554.37f }, { "D5", 587.33f }, { "D#5", 622.25f }, { "E5", 659.25f }, { "F5", 698.46f }, { "F#5", 739.99f }, { "G5", 783.99f }, { "G#5", 830.61f }, { "A5", 880.00f }, { "A#5", 932.33f }, { "B5", 987.77f },
            { "C6", 1046.50f }, { "C#6", 1108.73f }, { "D6", 1174.66f }, { "D#6", 1244.51f }, { "E6", 1318.51f }, { "F6", 1396.91f }, { "F#6", 1479.98f }, { "G6", 1567.98f }, { "G#6", 1661.22f }, { "A6", 1760.00f }, { "A#6", 1864.66f }, { "B6", 1975.53f },
            { "C7", 2093.00f }, { "C#7", 2217.46f }, { "D7", 2349.32f }, { "D#7", 2489.02f }, { "E7", 2637.02f }, { "F7", 2793.83f }, { "F#7", 2959.96f }, { "G7", 3135.96f }, { "G#7", 3322.44f }, { "A7", 3520.00f }, { "A#7", 3729.31f }, { "B7", 3951.07f },
            { "C8", 4186.01f }
        };
        public static readonly Dictionary<string,RawSourceWaveStream> ActiveNotes = new()
        {
        };
        public static void InitActiveNotes(int durationInMiliSeconds, int sampleRate = 44000)
        {
          foreach (var note in NoteFrequencies)
          {
                int bytesPerSample = 2; // 16-bit audio
                int totalSamples = (int)(sampleRate * durationInMiliSeconds / 1000);

                double[] amplitudes = { 1.0, 0.6, 0.4, 0.3, 0.2, 0.15, 0.1, 0.08 };
                byte[] buffer = new byte[totalSamples * bytesPerSample];
                double decayRate = 3; // Higher = faster damping
                                      // Generate the sound wave
                for (int i = 0; i < totalSamples; i++)
                {
                    double time = (double)i / sampleRate;
                    double envelope = Math.Exp(-decayRate * time);
                    double sampleValue = amplitudes[0] * Math.Sin(2 * Math.PI * note.Value * time);
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
                    buffer[i * bytesPerSample + 1] = (byte)((sample >> 8) & 0xFF);
                }

                // Create wave file and play

                var waveProvider = new RawSourceWaveStream(new MemoryStream(buffer), new NAudio.Wave.WaveFormat(sampleRate, 16, 1)); // 1 second duration, can be adjusted as needed
                ActiveNotes.Add(note.Key, waveProvider);
            }
        }
        public static float GetFrequency(string note)
        {
            if (NoteFrequencies.TryGetValue(note, out float frequency))
            {
                return frequency;
            }
            throw new ArgumentException($"Note {note} is not valid.");
        }
        public static RawSourceWaveStream GetNoteBuffer(string note)
        {
            if (ActiveNotes.TryGetValue(note, out RawSourceWaveStream buffer))
            {
                return buffer;
            }
            throw new ArgumentException($"Note {note} is not valid.");
        }
    }
}
