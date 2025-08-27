
using NAudio.Wave;
using NAudio.Midi;
using System.IO;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
namespace WPF_Piano
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    //                         <StackPanel x:Name="WhiteButtonLayout" Orientation="Horizontal" VerticalAlignment="Center" HorizontalAlignment="Center"/>
    //                     <StackPanel Panel.ZIndex="1" x:Name= "BlackButtonLayout" Orientation= "Horizontal" VerticalAlignment= "Center" HorizontalAlignment= "Center" />



    public partial class MainWindow : Window
    {
        public Random rand = new Random();
        PianoUIRender PianoUIRender = new PianoUIRender();
        PianoSettings pianoButtonSettings = new PianoSettings();
        Dictionary<string, string> pianoMapping = new Dictionary<string, string>();
        MidiFile midiFile;
        public MainWindow()
        {
            InitializeComponent();
            // Set up the key event handler for the window
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            this.KeyUp += UnhighlightKey;
            pianoMapping = pianoButtonSettings.ReturnPianoMapping();
            PianoUIRender.RenderButton(this,PianoButtonOctave);
      
            
        }
        public void RenderNoteTile(FrameworkElement Fe,Canvas tileFrame, int timeInSecond)
        {
            
            for (int i=0; i < 10; i++)
            {
                var noteTile = new Control
                {
                    Template = Fe.FindResource("NoteTile") as ControlTemplate,
                    Width = 50,
                    Height = 50,

                }; Canvas.SetLeft(noteTile, rand.NextInt64(100,500));
                Canvas.SetTop(noteTile, 0);
                myCanvas.Children.Add(noteTile);
                // Create storyboard
           
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = tileFrame.ActualHeight + noteTile.Height,
                    Duration = TimeSpan.FromSeconds(2)
                };
                var Storyboard = new Storyboard();
                Storyboard.SetTarget(animation, noteTile);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));
                Storyboard.Children.Add(animation);
                Storyboard.Begin();
            
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //AnimateFallingTile();
        }
        

        public void Key_Pressed(object sender, KeyEventArgs e)
        {
            string keyName = e.Key.ToString();
            // if keyname is number layout ( such as D1 ) , convert it to "1"
            if (keyName.StartsWith("D") && keyName.Length > 1 && char.IsDigit(keyName[1]))
            {
                keyName = keyName.Substring(1);
            }
            string noteName = pianoMapping.ContainsKey(keyName) ? pianoMapping[keyName] : null;
          
            if (string.IsNullOrEmpty(noteName))
            {

                return;
            }
            float noteFrequency = NoteValue.NoteFrequencies.ContainsKey(noteName) ? NoteValue.NoteFrequencies[noteName] : 0;

            Task.Run(() => PlaySound(noteFrequency, 1));
        }
        public void PlaySound(float frequency, int durationInSeconds, int sampleRate = 44100)
        {


            // Sound settings
            //double amplitude = 1; // Volume level (0.0 to 1.0)
            int bytesPerSample = 2; // 16-bit audio
            int totalSamples = sampleRate * durationInSeconds;

            double[] amplitudes = { 1.0, 0.6, 0.4, 0.3, 0.2, 0.15, 0.1, 0.08 };
            byte[] buffer = new byte[totalSamples * bytesPerSample];
            double decayRate = 4.0; // Higher = faster damping
                                    // Generate the sound wave
            for (int i = 0; i < totalSamples; i++)
            {
                double time = (double)i / sampleRate;
                double envelope = Math.Exp(-decayRate * time);
                double sampleValue = amplitudes[0] * Math.Sin(2 * Math.PI * frequency * time);
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
            using var waveoutEvent = new WaveOutEvent();
            using var waveProvider = new RawSourceWaveStream(new MemoryStream(buffer), new NAudio.Wave.WaveFormat(sampleRate, 16, 1));

            waveoutEvent.Init(waveProvider);
            waveoutEvent.Play();

            // Thread sleep
            while (waveoutEvent.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(100);
            }

        }
        public void HighlightKey(object sender, KeyEventArgs e)
        {
            CustomPianoButton button = FindName(e.Key.ToString()) as CustomPianoButton;
            if (button != null)
            {
                button.Background = Brushes.Yellow;
            }
        }
        public void UnhighlightKey(object sender, KeyEventArgs e)
        {
            CustomPianoButton button = FindName(e.Key.ToString()) as CustomPianoButton;
            if (button != null)
            {
                button.Background = Brushes.White;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Empty;
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "MIDI files (*.mid)|*.mid|All files (*.*)|*.*";
                dialog.Title = "Select a MIDI File";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                     filePath = dialog.FileName;
                }
            }
            //Read MIDI file and play it
             midiFile = new MidiFile(filePath, false);
             var noteInfo = ExtractNoteInfo(midiFile);
        }
        public List<NoteTileInfo> ExtractNoteInfo(MidiFile midiFile)
        {
            var noteTiles = new List<NoteTileInfo>();
            var deltaPerTick = midiFile.DeltaTicksPerQuarterNote;
            
            var tempo = midiFile.Events[0].OfType<TempoEvent>().FirstOrDefault().MicrosecondsPerQuarterNote;
            int noteCount = midiFile.Events.OfType<NoteOnEvent>().Count(); // Exclude the first 3 meta events and last end track event
           
            for (int i = 3; i < noteCount; i=i+2)
            {
                var midiEvent = midiFile.Events[1][i];
                if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                {
                    var noteOnEvent = (NoteOnEvent)midiEvent;
                    var noteTile = new NoteTileInfo
                    {
                        NoteNumber = noteOnEvent.NoteNumber,
                        Duration =(noteOnEvent.OffEvent.AbsoluteTime - noteOnEvent.AbsoluteTime) * tempo / (deltaPerTick * 1_000_000.0)
                    };
                    noteTiles.Add(noteTile);
                }
            }
 
            return noteTiles;
        }
        public class NoteTileInfo
        {
            public int NoteIndex { get; set; } 
            public int NoteLength { get; set; }
            public Color NoteColor { get; set; } = Colors.Blue;
            // Start time in second
            public double StartTime { get; set; }
            public double Duration { get; set; }
            public int NoteNumber { get; set; }



        }
    }
}