using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace WPF_Piano.Deprecated
{
    public class NoteTileInfo
    {
        public string NoteName { get; set; }
        //public int NoteLength { get; set; }
        public MetricTimeSpan StartTime { get; set; }
        public int Velocity { get; set; }
        public MetricTimeSpan Duration { get; set; }
        public static Queue<NoteTileInfo> ExtractNoteInfo(MidiFile midiFile)
        {
            var noteTiles = new Queue<NoteTileInfo>();
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
                    Velocity = (int)note.Length / 10 // Scale velocity to fit in UI height (0-127 to 0-60)
                };
                noteTiles.Enqueue(noteTile);
            }
            return noteTiles;
        }

    }
}
