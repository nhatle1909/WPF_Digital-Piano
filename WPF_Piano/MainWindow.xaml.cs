

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
using WPF_Piano.StaticValue;
namespace WPF_Piano
{
    public partial class MainWindow : Window
    {
       
        PianoUIRender PianoUIRender = new PianoUIRender();
        PianoSettings pianoButtonSettings = new PianoSettings();
        PianoPlaySound PianoPlaySound = new PianoPlaySound();
        Dictionary<string, string> pianoMapping = new Dictionary<string, string>();
        MidiFile midiFile;
        Playback playback;
    
        public MainWindow()
        {
            InitializeComponent();
            // Set up the key event handler for the window
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            this.KeyUp += UnhighlightKey;
            pianoMapping = pianoButtonSettings.ReturnPianoMapping();
            PianoUIRender.RenderButton(this,PianoButtonOctave);
            PianoUIRender.RenderNoteTileFrame(this,NoteTileFrame,true);
            var songs = new List<Song>
    {
        new Song { Name = "Perfect Tears", Description = "Riryka - Emotional Ballad", Icon = "/Resources/song1.png" },
        new Song { Name = "Night Drive", Description = "Lo-fi Chill Mix", Icon = "/Resources/song2.png" }
    };

            SongList.ItemsSource = songs;
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
            
            Task.Run(()=> PianoPlaySound.PlaySound(noteFrequency,1000));

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
                background.Background = button.Name.Contains("Black") ? Brushes.Black: Brushes.White ;
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
      
        public async void PlayTest(object sender, RoutedEventArgs e)
        {
            if (midiFile == null)
            {
                MessageBox.Show("Please choose a music file before playing");
                return;
            }
            if (playback != null && playback.IsRunning)
            {
                playback.Stop();
                playback.Dispose();
            }
            var noteTiles = NoteTileInfo.ExtractNoteInfo(midiFile);
            var noteIndex = 0;
            playback = midiFile.GetPlayback();
            playback.MoveToFirstSnapPoint();
            playback.Speed = 1;

            playback.Start();
            playback.EventPlayed += async (obj, args) =>
            {
                if (args.Event is NoteOnEvent noteOnEvent)
                {
                    Dispatcher.Invoke(() =>
                    {
                        //NoteShow.Text = $" Note Name: {GetNoteName(noteOnEvent.NoteNumber)} Velocity: {noteOnEvent.Velocity}";
                        PianoUIRender.RenderNoteTile(this, NoteTileFrame, noteTiles[noteIndex]);
                    });

                    _ = Task.Run(() => PianoPlaySound.PlaySound(NoteValue.GetFrequency(GetNoteName(noteOnEvent.NoteNumber)),
                        1000));

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

        public class Song
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
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
     
    }
}