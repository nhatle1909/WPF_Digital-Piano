
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
        double yEnhancedScale = 0.1;
            
        MidiEventCollection midiEventCollection;
        public NoteControl()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RenderTileBorder();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        private void RenderTileBorder()
        {

            var noteCount = PianoSettings.Instance.PianoMapping.Count;
            var octaveCount = (int)Math.Ceiling(noteCount / 12.0);
            for (int i = 1; i < octaveCount; i++) // Use <= to get the last border
            {
                Rectangle border = new Rectangle
                {
                    Width = 2,
                    Height = 10000,
                    Fill = Brushes.Blue, // Use Fill for a solid 2px line
                    SnapsToDevicePixels = true
                };

                // Set scaling hint to keep lines sharp in Viewbox
                RenderOptions.SetEdgeMode(border, EdgeMode.Aliased);

                // Align exactly to the 420px octave boundary
                Canvas.SetLeft(border, (i * 420 - 2));
                Canvas.SetBottom(border, 0);
                NoteCanvas.Children.Add(border);
            }

        }
        public MidiEventCollection MidiEventCollection
        {
            get
            {
                return midiEventCollection;
            }
            set
            {
                yScale = (20.0 / value.DeltaTicksPerQuarterNote);
                midiEventCollection = value;
                NoteCanvas.Children.Clear();

                long lastPosition = 0;

                for (int track = 0; track < midiEventCollection.Tracks; track++)
                {
                    // Inside your MidiEventCollection set logic:
                    foreach (MidiEvent midiEvent in value[track])
                    {
                        if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                        {
                            NoteOnEvent noteOn = (NoteOnEvent)midiEvent;
                            if (noteOn.OffEvent != null)
                            {
                                string note = PianoPlaySound.Instance.GetNoteName(noteOn.NoteNumber);
                                if (!PianoSettings.Instance.CheckNote(note))
                                {
                                    continue;
                                }

                                FrameworkElement tile = MakeNoteBorder(note, noteOn.NoteNumber, noteOn.AbsoluteTime, noteOn.NoteLength, noteOn.Channel);
                                NoteCanvas.Children.Add(tile);

                                lastPosition = Math.Max(lastPosition, noteOn.AbsoluteTime + noteOn.NoteLength);
                            }
                        }
                    }
                }
                this.Height = lastPosition * yScale * 2 * 1.275;
                // 2 is the enhancedYScale multiplier, 1.275  = 1.25 (vertical spread) * 1.02 (extra padding for visual comfort)
                this.Width = 128 * xScale;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    RenderTileBorder();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        private FrameworkElement MakeNoteBorder(string noteName, int noteNumber, long startTime, int duration, int channel)
        {

            bool isBlack = noteName.Contains("#");
            double enhancedYScale = yScale * 2.0;

            // Create the Border control
            Border noteTile = new Border
            {
                // Width and Height logic (matching your previous specific 26/56 offset)
                Width = isBlack ? 30 : 60,
                Height = (double)duration * enhancedYScale,

                // Border styling
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6), // Adds slightly rounded corners
                BorderBrush = new SolidColorBrush(isBlack ? Color.FromRgb(0, 255, 255) : Color.FromRgb(0, 200, 255)),

                // Inner fill
                Background = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),

                // Optimization
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

            // Position on Canvas
            Canvas.SetLeft(noteTile, (octave * 420) + xOffset);
            // Applying your 1.25x vertical spread multiplier
            Canvas.SetBottom(noteTile, (double)startTime * enhancedYScale * 1.25);

            // Visual Effect (Glow)
            noteTile.Effect = new DropShadowEffect
            {
                Color = ((SolidColorBrush)noteTile.BorderBrush).Color,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.6
            };

            return noteTile;
        }
       
    }
}
