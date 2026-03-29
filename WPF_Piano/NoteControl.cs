using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WPF_Piano.Helper;

namespace WPF_Piano
{
    public partial class NoteControl : FrameworkElement
    {
        private readonly VisualCollection _visuals;
        private readonly DrawingVisual _drawingVisual;

        // Visual Constants
        private const double X_SCALE = 30;
        private const int PIXELS_PER_SECOND = 150;
        private double _yScale = 0.1;
        private double _songDuration = 10;

        public static readonly DependencyProperty MidiFileProperty =
            DependencyProperty.Register(
                nameof(MidiFile),
                typeof(MidiFile),
                typeof(NoteControl),
                new PropertyMetadata(null, (d, e) =>
                {
                    if (d is NoteControl control && e.NewValue is MidiFile midi)
                    {
                        control.UpdateMetricsAndRender(midi);
                    }
                }));

        public MidiFile MidiFile
        {
            get => (MidiFile)GetValue(MidiFileProperty);
            set => SetValue(MidiFileProperty, value);
        }

        public NoteControl()
        {
            _visuals = new VisualCollection(this);
            _drawingVisual = new DrawingVisual();
            _visuals.Add(_drawingVisual);

            // Set initial default size
            this.Width = 128 * X_SCALE;
            this.Height = _songDuration * PIXELS_PER_SECOND;
            Render();
        }

        // Required overrides for WPF to recognize the DrawingVisual
        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        private void UpdateMetricsAndRender(MidiFile midi)
        {
            // Calculate timing scales
            _yScale = (20.0 / midi.DeltaTicksPerQuarterNote) * 2;
            _songDuration = PianoPlaySound.Instance.CalculateSongDuration(midi);
            _visuals.Clear();
            _visuals.Add(_drawingVisual);
            // Calculate total height based on last note
            long lastPosition = 0;
            foreach (var track in midi.Events)
            {
                foreach (var e in track)
                {
                    if (e is NoteOnEvent n)
                        lastPosition = Math.Max(lastPosition, n.AbsoluteTime + n.NoteLength);
                }
            }

            this.Height = lastPosition * _yScale * 1.28;
            this.Width = 128 * X_SCALE;

            Render();
        }

        private void Render()
        {
            using (DrawingContext dc = _drawingVisual.RenderOpen())
            {
                // 1. Draw Background Grid & Timelines
                DrawGrid(dc);

                // 2. Draw Midi Notes
                if (MidiFile != null)
                {
                    DrawNotes(dc);
                }
            }
        }

        private void DrawGrid(DrawingContext dc)
        {
            // Draw Octave Dividers (Vertical)
            var noteCount = PianoSettings.Instance.PianoMapping.Count;
            var octaveCount = (int)Math.Ceiling(noteCount / 12.0);
            double octaveWidth = 2520 / (double)octaveCount;
            Pen borderPen = new Pen(Brushes.Gray, 2);
            borderPen.Freeze();

            for (int i = 1; i < octaveCount; i++)
            {
                double x = i * octaveWidth;
                x = i >= 2 ? x - 2 : x;
                dc.DrawLine(borderPen, new Point(x, 0), new Point(x, this.Height));
            }

            // Draw Timelines (Horizontal)
            Pen timelinePen = new Pen(Brushes.LimeGreen, 2) { DashStyle = DashStyles.Dash };
            timelinePen.Freeze();

            for (int s = 0; s <= _songDuration; s++)
            {
                double y = this.Height - (s * PIXELS_PER_SECOND);
                dc.DrawLine(timelinePen, new Point(0, y), new Point(this.Width, y));

                var text = new FormattedText(
                    $"{s}s",
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    20,
                    Brushes.LimeGreen,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                dc.DrawText(text, new Point(5, y - 25));
            }
        }

        private void DrawNotes(DrawingContext dc)
        {
            Brush noteBrush = new SolidColorBrush(Color.FromArgb(160, 0, 255, 255));
            Pen notePen = new Pen(new SolidColorBrush(Color.FromRgb(0, 200, 255)), 2);
            noteBrush.Freeze();
            notePen.Freeze();

            foreach (var track in MidiFile.Events)
            {
                foreach (var midiEvent in track)
                {
                    if (midiEvent is NoteOnEvent noteOn && noteOn.OffEvent != null)
                    {
                        if (noteOn.NoteNumber < 36) continue;

                        int relativeNote = noteOn.NoteNumber - 36;
                        double xOffset = (relativeNote % 12) switch
                        {
                            0 => 0,
                            1 => 45,
                            2 => 60,
                            3 => 105,
                            4 => 120,
                            5 => 180,
                            6 => 225,
                            7 => 240,
                            8 => 285,
                            9 => 300,
                            10 => 345,
                            11 => 360,
                            _ => 0
                        };

                        double x = ((relativeNote / 12) * 420) + xOffset;
                        bool isBlack = PianoPlaySound.Instance.GetNoteName(noteOn.NoteNumber).Contains("#");
                        double width = isBlack ? 30 : 60;
                        double height = noteOn.NoteLength * _yScale;

                        // Coordinate flip: start from bottom
                        double y = this.Height - (noteOn.AbsoluteTime * _yScale * 1.25) - height;

                        dc.DrawRoundedRectangle(noteBrush, notePen, new Rect(x, y, width, height), 6, 6);
                    }
                }
            }
        }
    }
}
