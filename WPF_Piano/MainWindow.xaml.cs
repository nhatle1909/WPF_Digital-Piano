

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace WPF_Piano
{
    public partial class MainWindow : Window
    {
        public Random rand = new Random();
        PianoUIRender PianoUIRender = new PianoUIRender();
        PianoSettings pianoButtonSettings = new PianoSettings();
        PianoPlaySound PianoPlaySound = new PianoPlaySound();
        Dictionary<string, string> pianoMapping = new Dictionary<string, string>();
        MidiFile midiFile;
        Playback playback;
        MixingSampleProvider bufferProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 1)) { ReadFully = true};
        WasapiOut output = new WasapiOut(AudioClientShareMode.Shared, false, 20);
        public MainWindow()
        {
            InitializeComponent();
            // Set up the key event handler for the window
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            this.KeyUp += UnhighlightKey;
            this.KeyUp += Key_Released;
            pianoMapping = pianoButtonSettings.ReturnPianoMapping();
            PianoUIRender.RenderButton(this,PianoButtonOctave);
            PianoUIRender.RenderNoteTileFrame(this,NoteTileFrame,true);
            output.Init(bufferProvider);
            output.Play();
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
            
            Task.Run(()=>PlaySound(noteFrequency,1000));
            Dispatcher.Invoke(() =>
            {
                NoteShow.Text = $" Note Name: {noteName} Frequency: {noteFrequency}Hz";
                PianoUIRender.RenderNoteTile(this, NoteTileFrame, new NoteTileInfo { NoteName = noteName, StartTime = new MetricTimeSpan(0, 0, 0), Duration = new MetricTimeSpan(0, 0, 500), Velocity = 60 });
            });

        }
        public void Key_Released(object sender, KeyEventArgs e)
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

        }
        public void PlaySound(float frequency, int durationInMiliSeconds, int sampleRate = 44100)
        {

                // Sound settings
                //double amplitude = 1; // Volume level (0.0 to 1.0)
                int bytesPerSample = 2; // 16-bit audio
                int totalSamples = (int)(sampleRate * durationInMiliSeconds / 1000 );

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
        public static FrameworkElement FindElementByName(DependencyObject parent, string name)
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.Name == name)
                    return fe;

                var result = FindElementByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        public void HighlightKey(object sender, KeyEventArgs e)
        {
            var buttonPressed = e.Key.ToString();
            // if keyname is number layout ( such as D1 ) , convert it to "1"
            var button = FindTheButton(buttonPressed);
            var background =FindElementByName(button,"Background") as Button;
            if (background != null)
            {
                background.Background = Brushes.Yellow;
            }
     
        }
        public void UnhighlightKey(object sender, KeyEventArgs e)
        {
            var buttonPressed = e.Key.ToString();
            // if keyname is number layout ( such as D1 ) , convert it to "1"
                     var button = FindTheButton(buttonPressed);

            if (button == null) return;
            var background = FindElementByName(button, "Background") as Button;
            if (background != null)
            {
                background.Background = Brushes.White;
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
            if (filePath == string.Empty) return;
            //Read MIDI file 
            midiFile = MidiFile.Read(filePath);
 
        }
        public List<NoteTileInfo> ExtractNoteInfo(MidiFile midiFile)
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
        public async void PlayTest(object sender, RoutedEventArgs e)
        {
            if (playback != null && playback.IsRunning)
            {
                playback.Stop();
                playback.Dispose();
            }
            var noteTiles = ExtractNoteInfo(midiFile);
            var noteIndex = 0;
            playback = midiFile.GetPlayback();
         
            playback.Speed = 1;

            playback.Start();
            playback.EventPlayed += async (obj, args) =>
            {
                if (args.Event is NoteOnEvent noteOnEvent)
                {
                    Dispatcher.Invoke(() =>
                    {
                        NoteShow.Text = $" Note Name: {GetNoteName(noteOnEvent.NoteNumber)} Velocity: {noteOnEvent.Velocity}";
                        PianoUIRender.RenderNoteTile(this, NoteTileFrame, noteTiles[noteIndex]);
                    });

                    //Task.Run(() => PlaySound(NoteValue.GetFrequency(GetNoteName(noteOnEvent.NoteNumber)), noteTiles[noteIndex].Duration.Milliseconds));
                  
                    noteIndex++;


                }
            };
        }

        public string GetNoteName(int noteNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (noteNumber / 12) - 1;
            string name = noteNames[noteNumber % 12];
            return $"{name}{octave}";
        }


        public Button FindTheButton(string keyPressed)
        {
            // if keyname is number layout ( such as D1 ) , convert it to "1"
            if (keyPressed.StartsWith("D") && keyPressed.Length > 1 && char.IsDigit(keyPressed[1]))
            {
                keyPressed = keyPressed.Substring(1);
            }
            if (!pianoMapping.ContainsKey(keyPressed)) return null;
            string key = pianoMapping[keyPressed].Contains("#") ? $"Black{keyPressed}" : $"White{keyPressed}";
            CustomPianoButton button = FindElementByName(this, key) as CustomPianoButton;
            return button;
        }
        public class NoteTileInfo
        {
           public string NoteName { get; set; }
           //public int NoteLength { get; set; }
           public MetricTimeSpan StartTime { get; set; }
           public int Velocity { get; set; }
           public MetricTimeSpan Duration { get; set; }
        }
    }
}