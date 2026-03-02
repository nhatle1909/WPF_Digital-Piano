

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPF_Piano.Deprecated;
using WPF_Piano.Helper;
using WPF_Piano.ViewModel;

namespace WPF_Piano
{
    public partial class MainWindow : Window
    {


        public MainViewVM MainViewVM = new();
        private MidiFile midiFile;
        private Playback playback;
        private Queue<NoteTileInfo> noteTiles;
        public MainWindow()
        {
            InitializeComponent();
            // Set up the key event handler for the window
            this.DataContext = MainViewVM;
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            this.KeyUp += UnhighlightKey;
            PianoUIRender.Instance.RenderButton(this, PianoButtonOctave);

        }

        public void Key_Pressed(object sender, KeyEventArgs e)
        {
            MainViewVM.PianoButtonVM.PlayNote(e);

        }


        public void HighlightKey(object sender, KeyEventArgs e)
        {
            var buttonPressed = e.Key.ToString();
            // if keyname is number layout ( such as D1 ) , convert it to "1"
            var button = FEHelper.FindTheButton(this, buttonPressed);
            var background = FEHelper.FindElementByName(button, "Background") as Button;
            if (background != null)
            {
                background.Background = Brushes.Yellow;
            }

        }
        public void UnhighlightKey(object sender, KeyEventArgs e)
        {
            var buttonPressed = e.Key.ToString();
            // if keyname is number layout ( such as D1 ) , convert it to "1"
            var button = FEHelper.FindTheButton(this, buttonPressed);

            if (button == null) return;

            var background = FEHelper.FindElementByName(button, "Background") as Button;
            if (background != null)
            {
                background.Background = button.Name.Contains("Black") ? Brushes.Black : Brushes.White;
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
                    //midiNoteControl = new UserControl1();
                    //midiNoteControl.MidiEvents = new NAudio.Midi.MidiFile(filePath, true).Events;
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

            playback = midiFile.GetPlayback();

            playback.Speed = 1;

            playback.EventPlayed += (obj, args) =>
            {
                if (args.Event is NoteOnEvent noteOnEvent)
                {
                    var noteTile = noteTiles.Dequeue();
                    Dispatcher.Invoke(() =>
                    {

                        //PianoUIRender.RenderNoteTile(this, NoteTileFrame, noteTile);
                    });

                    _ = Task.Run(() => PianoPlaySound.Instance.PlaySound(NoteValue.GetFrequency(GetNoteName(noteOnEvent.NoteNumber)), 1000)); // new

                }
            };
            playback.Start();
        }
        public string GetNoteName(int noteNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (noteNumber / 12) - 1;
            string name = noteNames[noteNumber % 12];
            return $"{name}{octave}";
        }

    }
}