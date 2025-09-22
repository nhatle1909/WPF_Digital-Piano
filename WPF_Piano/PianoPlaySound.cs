using NAudio.Wave;
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
    public class PianoPlaySound : WaveProvider32
    {
        public class ActiveNotes
        {
            public float Frequency;
            public float Amplitude;
            public double Phase;
            public bool IsOn;
            public double TimeAlive;
        }
        public PianoPlaySound()
        {
            SetWaveFormat(44100, 1); // 44.1kHz mono
        }
        private readonly List<ActiveNotes> activeNotes = new List<ActiveNotes>();
        private readonly object lockObj = new object();
        private readonly int SampleRate = 44100;
        private readonly double decayRate = 3.0; // Decay rate for the exponential envelope

        private readonly BufferedWaveProvider buffer;

        private readonly WaveOutEvent output;
    
        public void NoteOn(string note,int velocity)
        {
            lock (lockObj)
            {
                if (activeNotes.Any(n => n.Frequency == NoteValue.NoteFrequencies[note] && n.IsOn))
                    return; // Note is already on
                activeNotes.Add(new ActiveNotes
                    {
                    Frequency = NoteValue.NoteFrequencies[note],
                    Amplitude = velocity / 127f,
                    Phase = 0.0,
                    IsOn = true,
                    TimeAlive = 0.0
                });
            }
        }
        public void NoteOff(string note)
        {
            lock (lockObj)
            {
                var activeNote = activeNotes.FirstOrDefault(n => n.Frequency == NoteValue.NoteFrequencies[note] && n.IsOn);
                if (activeNote != null)
                {
                    activeNote.IsOn = false; // Mark the note as off
                }
            }
        }
       
       

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            lock (lockObj)
            {
                Array.Clear(buffer, offset, sampleCount);

                foreach (var note in activeNotes.ToList())
                {
                    if (!note.IsOn) continue;

                    for (int i = 0; i < sampleCount; i++)
                    {
                        float timeStep = 1f / WaveFormat.SampleRate;
                        note.TimeAlive += timeStep;
                      

                        double envelope = Math.Exp(-decayRate * note.TimeAlive);
                        
                        float sample =  (float)Math.Sin(2 * Math.PI * note.Frequency * note.TimeAlive);
                        //for (int h = 0; h < harmonics.Length; h++)
                        //{
                        //    sample += (float)(harmonics[h] * Math.Sin(2 * Math.PI * note.Frequency * (h + 1) * note.TimeAlive));
                        //}

                        sample *= (float)envelope * note.Amplitude;
                        sample = Math.Clamp(sample, -1f, 1f);
                        buffer[offset + i] += sample;

                        note.Phase += 2 * Math.PI * note.Frequency / WaveFormat.SampleRate;
                        if (note.Phase > 2 * Math.PI) note.Phase -= 2 * Math.PI;
                    }
                }

                activeNotes.RemoveAll(n => !n.IsOn);
            }

            return sampleCount;
        }
    }
}
