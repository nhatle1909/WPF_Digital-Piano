using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WPF_Piano.Helper;

namespace WPF_Piano
{
    public partial class NoteControl : FrameworkElement
    {
        private const double X_SCALE = 30;
        private const int PIXELS_PER_SECOND = 200;
        private static readonly int[] SemitoneOffsets = { 0, 45, 60, 105, 120, 180, 225, 240, 285, 300, 345, 360 };

        private static readonly Brush NoteBrush = Freezable(new SolidColorBrush(Color.FromArgb(160, 0, 255, 255)));
        private static readonly Pen NotePen = Freezable(new Pen(new SolidColorBrush(Color.FromRgb(0, 200, 255)), 2));
        private static readonly Pen BorderPen = Freezable(new Pen(Brushes.Gray, 2));

        // Timeline Pen & Brush for second markers
        private static readonly Pen TimelinePen = Freezable(new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)), 1));
        private static readonly Brush TimelineBrush = Freezable(new SolidColorBrush(Colors.LimeGreen));
        private static readonly Typeface TimelineTypeface = new("Segoe UI");

        private readonly VisualCollection _visuals;
        private readonly DrawingVisual _gridVisual = new();
        private readonly List<NoteInfo> _noteInfos = new();
        private double _songDuration;

        private static T Freezable<T>(T item) where T : Freezable { item.Freeze(); return item; }

        public static readonly DependencyProperty MidiFileProperty =
            DependencyProperty.Register(nameof(MidiFile), typeof(MidiFile), typeof(NoteControl),
                new PropertyMetadata(null, (d, e) =>
                {
                    if (d is NoteControl control && e.NewValue is MidiFile midi)
                        control.PrepareMidiAsync(midi);
                }));

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(double), typeof(NoteControl),
                new PropertyMetadata(0.0, (d, e) =>
                {
                    if (d is NoteControl control)
                        control.Render((double)e.NewValue);
                }));

        public MidiFile MidiFile { get => (MidiFile)GetValue(MidiFileProperty); set => SetValue(MidiFileProperty, value); }
        public double CurrentTime { get => (double)GetValue(CurrentTimeProperty); set => SetValue(CurrentTimeProperty, value); }

        public NoteControl()
        {
            _visuals = new VisualCollection(this) { _gridVisual };
            Width = 128 * X_SCALE;
            Height = 1920;

            PianoSettings.Instance.OctaveUpdated += OnOctaveChanged;
            SizeChanged += (_, _) => RedrawGridAndNotes();
        }

        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private void OnOctaveChanged()
        {
            if (_noteInfos.Count == 0) return;
            RecalculateNotePositions();
            RedrawGridAndNotes();
        }

        private async void PrepareMidiAsync(MidiFile midi)
        {
            var octave = PianoSettings.Instance.GetOctaveRange();
            var (notes, totalSeconds) = await Task.Run(() => BuildNoteInfos(midi, octave));

            Dispatcher.Invoke(() =>
            {
                _songDuration = totalSeconds;
                _noteInfos.Clear();
                _noteInfos.AddRange(notes);

                Height = Math.Max(200, totalSeconds * PIXELS_PER_SECOND);
                Width = 128 * X_SCALE;

                RedrawGridAndNotes();
            });
        }

        private (List<NoteInfo> Notes, double TotalSeconds) BuildNoteInfos(MidiFile midi, dynamic octave)
        {
            var notes = new List<NoteInfo>();
            int ticksPerQuarter = midi.DeltaTicksPerQuarterNote;
            var ordered = midi.Events.SelectMany(t => t).OrderBy(e => e.AbsoluteTime).ToList();

            var segments = new List<(long Tick, double Seconds, double Bpm)> { (0, 0.0, 120.0) };
            double currentBpm = 120.0, cumulativeSeconds = 0.0;
            long lastTick = 0;

            foreach (var ev in ordered)
            {
                if (ev.AbsoluteTime > lastTick)
                {
                    cumulativeSeconds += (ev.AbsoluteTime - lastTick) / (double)ticksPerQuarter * (60.0 / currentBpm);
                    lastTick = ev.AbsoluteTime;
                }
                if (ev is TempoEvent te)
                {
                    currentBpm = te.Tempo;
                    segments.Add((ev.AbsoluteTime, cumulativeSeconds, currentBpm));
                }
            }

            double TickToSec(long tick)
            {
                int lo = 0, hi = segments.Count - 1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (segments[mid].Tick == tick) { lo = mid; break; }
                    if (segments[mid].Tick < tick) lo = mid + 1; else hi = mid - 1;
                }
                var seg = segments[Math.Max(0, lo - 1)];
                return seg.Seconds + (tick - seg.Tick) / (double)ticksPerQuarter * (60.0 / seg.Bpm);
            }

            long lastPosition = 0;
            foreach (var noteOn in ordered.OfType<NoteOnEvent>().Where(n => n.OffEvent != null))
            {
                lastPosition = Math.Max(lastPosition, noteOn.AbsoluteTime + noteOn.NoteLength);

                double start = TickToSec(noteOn.AbsoluteTime);
                double duration = Math.Max(0.0, TickToSec(noteOn.AbsoluteTime + noteOn.NoteLength) - start);
                bool isBlack = (noteOn.NoteNumber % 12) is 1 or 3 or 6 or 8 or 10;

                notes.Add(new NoteInfo
                {
                    NoteNumber = noteOn.NoteNumber,
                    Start = start,
                    Duration = duration,
                    Height = duration * PIXELS_PER_SECOND,
                    IsBlack = isBlack
                });
            }

            CalculateNoteLayout(notes, octave);
            return (notes, TickToSec(lastPosition));
        }

        private void RecalculateNotePositions() =>
            CalculateNoteLayout(_noteInfos, PianoSettings.Instance.GetOctaveRange());

        private static void CalculateNoteLayout(IEnumerable<NoteInfo> notes, dynamic octave)
        {
            int octaveMax = int.Parse(octave.To[1].ToString());
            int octaveMin = int.Parse(octave.From[1].ToString());

            int minNoteNumber = (octaveMin + 1) * 12;
            int maxNoteNumber = (octaveMax + 2) * 12 - 1;
            double octaveWidth = 2520.0 / (octaveMax - octaveMin + 1);

            foreach (var note in notes)
            {
                if (note.NoteNumber < minNoteNumber || note.NoteNumber > maxNoteNumber)
                {
                    note.IsVisibleInOctave = false;
                    continue;
                }

                note.IsVisibleInOctave = true;
                int relativeNote = note.NoteNumber - minNoteNumber;
                int semitoneIndex = relativeNote % 12;

                double xOffset = (SemitoneOffsets[semitoneIndex] / 420.0) * octaveWidth;
                note.X = ((relativeNote / 12) * octaveWidth) + xOffset;
                note.Width = note.IsBlack ? (octaveWidth / 420.0) * 30.0 : (octaveWidth / 420.0) * 60.0;
            }
        }

        private void RedrawGridAndNotes()
        {
            DrawGrid();

            foreach (var note in _noteInfos)
            {
                note.Visual ??= new DrawingVisual();

                using (DrawingContext dc = note.Visual.RenderOpen())
                {
                    if (note.IsVisibleInOctave)
                    {
                        dc.DrawRoundedRectangle(
                            note.IsBlack ? Brushes.Black : NoteBrush,
                            NotePen,
                            new Rect(note.X, 0, Math.Max(0, note.Width - 5), note.Height),
                            6, 6);
                    }
                }
            }

            Render(CurrentTime);
        }

        private void Render(double currentTime)
        {
            if (_noteInfos.Count == 0) return;

            double viewportHeight = Math.Max(ActualHeight, 400.0);
            double windowStart = currentTime - 1.0;
            double windowEnd = currentTime + (viewportHeight / PIXELS_PER_SECOND) + 1.0;

            foreach (var note in _noteInfos)
            {
                bool isVisible = note.IsVisibleInOctave &&
                                 note.Start + note.Duration >= windowStart &&
                                 note.Start <= windowEnd;

                if (isVisible)
                {
                    double y = viewportHeight - (note.Start - currentTime) * PIXELS_PER_SECOND - note.Height;
                    note.Visual.Transform = new TranslateTransform(0, y);

                    if (!_visuals.Contains(note.Visual))
                        _visuals.Add(note.Visual);
                }
                else if (_visuals.Contains(note.Visual))
                {
                    _visuals.Remove(note.Visual);
                }
            }
        }

        private void DrawGrid()
        {
            var octave = PianoSettings.Instance.GetOctaveRange();
            int octaveMax = int.Parse(octave.To[1].ToString());
            int octaveMin = int.Parse(octave.From[1].ToString());
            int octaveCount = octaveMax - octaveMin + 1;
            double octaveWidth = 2520.0 / octaveCount;

            double targetHeight = Math.Max(Height, ActualHeight);
            if (Parent is FrameworkElement parentElement)
                targetHeight = Math.Max(targetHeight, parentElement.ActualHeight);

            double targetWidth = Math.Max(Width, ActualWidth);

            using (DrawingContext dc = _gridVisual.RenderOpen())
            {
                // 1. Draw Vertical Octave Grid Lines
                for (int i = 1; i < octaveCount; i++)
                {
                    double x = i >= 2 ? (i * octaveWidth) - 2 : i * octaveWidth;
                    dc.DrawLine(BorderPen, new Point(x, 0), new Point(x, targetHeight));
                }

                // 2. Draw Horizontal Timeline Grid Lines & Seconds Text
                double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                int maxSeconds = (int)Math.Ceiling(_songDuration);

                for (int s = 0; s <= maxSeconds; s++)
                {
                    double y = targetHeight - (s * PIXELS_PER_SECOND);
                    dc.DrawLine(TimelinePen, new Point(0, y), new Point(targetWidth, y));

                    var text = new FormattedText(
                        $"{s}s",
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        TimelineTypeface,
                        20,
                        TimelineBrush,
                        dpi);

                    dc.DrawText(text, new Point(5, y + 5));
                }
            }
        }

        private class NoteInfo
        {
            public int NoteNumber { get; init; }
            public double Start { get; init; }
            public double Duration { get; init; }
            public double Height { get; init; }
            public bool IsBlack { get; init; }

            public double X { get; set; }
            public double Width { get; set; }
            public bool IsVisibleInOctave { get; set; }
            public DrawingVisual Visual { get; set; }
        }
    }
}