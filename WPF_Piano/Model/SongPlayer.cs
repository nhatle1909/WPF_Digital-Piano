using NAudio.Midi;

namespace WPF_Piano.Model
{
    public class SongPlayer
    {
        public bool IsPlaying { get; set; }
        public double CurrentTime { get; set; }
        public double TotalDuration { get; set; }
        public MidiFile? CurrentMidiFile { get; set; }
    }
}