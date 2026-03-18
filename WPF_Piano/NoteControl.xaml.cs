
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Piano.Helper;

namespace WPF_Piano
{
    /// <summary>
    /// Interaction logic for NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {

        double xScale = 30;
        double yScale = 0.1;
        private MidiFile midiFile;
        private int pixelsPerSecond = 150;
        private double songDuration = 10;
        public MidiFile MidiFile
        {
            get
            {
                return midiFile;
            }
            set
            {
                NoteCanvas.Children.Clear();
                midiFile = value;
                yScale = (20.0 / value.DeltaTicksPerQuarterNote) * 2;
                songDuration = CalculateSongDuration();
                RenderTiles(value.Events);
                RenderTileBorder();
                RenderTimelines();
            }
        }

        public NoteControl()
        {
            InitializeComponent();
            this.Height = songDuration * pixelsPerSecond;
            this.Width = 128 * xScale;
            RenderTileBorder();
            RenderTimelines();
        }
        public void RenderTiles(MidiEventCollection midiEvents)
        {
            long lastPosition = 0;

            for (int track = 0; track < midiEvents.Tracks; track++)
            {
                foreach (MidiEvent midiEvent in midiEvents[track])
                {
                    if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        NoteOnEvent noteOn = (NoteOnEvent)midiEvent;
                        if (noteOn.OffEvent != null)
                        {
                            string note = PianoPlaySound.Instance.GetNoteName(noteOn.NoteNumber);

                            FrameworkElement tile = MakeNoteBorder(note, noteOn.NoteNumber, noteOn.AbsoluteTime, noteOn.NoteLength, noteOn.Channel);
                            NoteCanvas.Children.Add(tile);

                            lastPosition = Math.Max(lastPosition, noteOn.AbsoluteTime + noteOn.NoteLength);
                        }
                    }
                }
            }
            this.Height = lastPosition * yScale * 1.28;
            // 1.28 = 1.25 (vertical spread) + 0.03 (extra padding for visual comfort)
        }
        private void RenderTileBorder()
        {
            var noteCount = PianoSettings.Instance.PianoMapping.Count;
            var octaveCount = (int)Math.Ceiling(noteCount / 12.0);
            double octaveWidth = 2520 / octaveCount;
            for (int i = 1; i < octaveCount; i++)
            {
                double xPos = i * octaveWidth;

                Line border = new Line
                {
                    X1 = xPos - 2,
                    Y1 = 0,
                    X2 = xPos - 2,
                    Y2 = this.Height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    SnapsToDevicePixels = true
                };
                if (i == 1)
                {
                    border.X1 = xPos;
                    border.X2 = xPos;
                }
                if (i == octaveCount - 1)
                {
                    border.X1 = xPos - 4;
                    border.X2 = xPos - 4;
                }  
                RenderOptions.SetEdgeMode(border, EdgeMode.Aliased);
                NoteCanvas.Children.Add(border);
            }
        }
        public void RenderTimelines()
        {
            double totalSeconds = (midiFile == null) ? 10 : songDuration;
            for (int s = 0; s <= totalSeconds; s++)
            {
                double yPos = s * pixelsPerSecond;

                Line timeline = new Line
                {
                    X1 = 0,
                    X2 = this.Width,
                    Y1 = yPos,
                    Y2 = yPos,
                    Stroke = Brushes.Lime,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
  
                    SnapsToDevicePixels = true,
                    
                };
                if (midiFile != null && s > 0) {
                    TextBlock timeLabel = new TextBlock
                    {
                        Text = $"{s}s",
                        Foreground = Brushes.Lime,
                        FontSize = 20,
                  
                       
                    };
                    RenderOptions.SetEdgeMode(timeLabel, EdgeMode.Aliased);
                    Canvas.SetLeft(timeLabel, 5);
                    Canvas.SetBottom(timeLabel, yPos + 55);
                    NoteCanvas.Children.Add(timeLabel);
                }
                RenderOptions.SetEdgeMode(timeline, EdgeMode.Aliased);
             
                NoteCanvas.Children.Add(timeline);
            }
        }
        private double CalculateSongDuration()
        {
            long maxTick = 0;
            foreach (var track in midiFile.Events)
            {
                if (track.Count > 0)
                {
                    long lastEventTick = track.Max(e => e.AbsoluteTime);
                    if (lastEventTick > maxTick) maxTick = lastEventTick;
                }
            }

            int ticksPerQuarterNote = midiFile.DeltaTicksPerQuarterNote;

          
            double bpm = 120.0;
            foreach (var track in midiFile.Events)
            {
                var tempoEvent = track.OfType<TempoEvent>().FirstOrDefault();
                if (tempoEvent != null)
                {
                    bpm = tempoEvent.Tempo;
                    break;
                }
            }
            double ticksPerSecond = (ticksPerQuarterNote * bpm) / 60.0;

            return  (double)maxTick / ticksPerSecond;
        }
        private FrameworkElement MakeNoteBorder(string noteName, int noteNumber, long startTime, int duration, int channel)
        {

            bool isBlack = noteName.Contains("#");
         
            Border noteTile = new Border
            {
              
                Width = isBlack ? 30 : 60,
                Height = (double)duration * yScale,

             
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6), 
                BorderBrush = new SolidColorBrush(isBlack ? Color.FromRgb(0, 255, 255) : Color.FromRgb(0, 200, 255)),

          
                Background = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),

          
                IsHitTestVisible = false
            };

            // Mapping Logic: relativeNote 0 = MIDI 36 (C2)
            int relativeNote = noteNumber - 36;
       
            int octave = relativeNote / 12;
            int noteInOctave = relativeNote % 12;

            double xOffset = noteInOctave switch
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

            Canvas.SetLeft(noteTile, (octave * 420) + xOffset);
            // Applying 1.25x vertical spread multiplier
            Canvas.SetBottom(noteTile, (double)startTime * yScale * 1.25);

            noteTile.Effect = new DropShadowEffect
            {
                Color = ((SolidColorBrush)noteTile.BorderBrush).Color,
                BlurRadius = 5,
                ShadowDepth = 0,
                Opacity = 0.6
            };

            return noteTile;
        }
       
    }
}
