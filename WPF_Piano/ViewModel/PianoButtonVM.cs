using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using WPF_Piano.Helper;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class PianoButtonVM : INotifyPropertyChanged
    {
        public ICommand PlayNoteCommand;
        public List<PianoButton> PianoButtons { get; set; } = new List<PianoButton>();
        private readonly Dictionary<string, string> pianoMapping = PianoSettings.Instance.PianoMapping;
        FrameworkElement Fe;
        StackPanel PianoButtonOctave;

        public PianoButtonVM(FrameworkElement fe, StackPanel pianoButtonOctave)
        {
            PlayNoteCommand = new RelayCommand(PlayNote);
            // Initialize piano buttons with their corresponding keys and frequencies
            foreach (var key in PianoSettings.Instance.PianoMapping.Keys)
            {
                string note = PianoSettings.Instance.PianoMapping[key];
                float frequency = NoteValue.GetFrequency(note);
                PianoButtons.Add(new PianoButton(note, key, frequency));
            }
            this.Fe = fe;
            this.PianoButtonOctave = pianoButtonOctave;
            RenderButton();
            PianoSettings.Instance.MappingUpdated += RenderButton;
        }
        public void RenderButton()
        {
            var octave = PianoSettings.Instance.GetOctaveRange();
            var firstOctaveIndex = Int32.Parse(octave.From.ToString().Last().ToString()) - 1;
            PianoButtonOctave.Children.Clear();
            for (int i = firstOctaveIndex; i < 6; i++)
            {
                var octaveGrid = new Grid
                {
                    Name = $"PianoOctave{i}",
                    Width = 420,
                    Height = 180,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var whiteLayout = new StackPanel { Orientation = Orientation.Horizontal };
                var blackLayout = new Canvas { IsHitTestVisible = false };

                octaveGrid.Children.Add(whiteLayout);
                octaveGrid.Children.Add(blackLayout);
                PianoButtonOctave.Children.Add(octaveGrid);

                for (int i2 = 1; i2 <= 12; i2++)
                {
                    var displayKeyAndNote = GetDisplayKeyAndNote(i, i2 - 1);
                    var keyName = pianoMapping.ElementAt(i2 - 1 + i * 12).Key;

                    if (new[] { 2, 4, 7, 9, 11 }.Contains(i2)) // Black Keys
                    {
                        var btn = new CustomPianoButton
                        {
                            Template = (ControlTemplate)Fe.FindResource("BlackPianoButton"),
                            Width = 30,
                            Height = 110,
                            KeyLabel = displayKeyAndNote.Item1,
                            NoteLabel = displayKeyAndNote.Item2,
                            Name = $"Black{keyName}",
                            FontSize = 10
                        };


                        double leftPos = i2 switch { 2 => 45, 4 => 105, 7 => 225, 9 => 285, 11 => 345, _ => 0 };
                        Canvas.SetLeft(btn, leftPos);
                        blackLayout.Children.Add(btn);
                    }
                    else // White Keys
                    {
                        var btn = new CustomPianoButton
                        {
                            Template = (ControlTemplate)Fe.FindResource("WhitePianoButton"),
                            Width = 60,
                            Height = 180,
                            KeyLabel = displayKeyAndNote.Item1,
                            NoteLabel = displayKeyAndNote.Item2,
                            Name = $"White{keyName}",
                            FontSize = 12
                        };
                        whiteLayout.Children.Add(btn);
                    }
                }
            }
        }
        public void PlayNote(object parameter)
        {
            if (parameter is KeyEventArgs e)
            {
                string keyName = e.Key.ToString();

                // if keyname is number layout ( such as D1 ) , convert it to "1"
                if (keyName.StartsWith("D") && keyName.Length > 1 && char.IsDigit(keyName[1]))
                {
                    keyName = keyName.Substring(1);
                }
                var pianoButton = PianoButtons.FirstOrDefault(pb => pb.Key == keyName);
                if (pianoButton != null)
                    PlayFrequency(pianoButton.Frequency);
            }
        }
        private void PlayFrequency(float frequency)
        {
            Task.Run(() => PianoPlaySound.Instance.PlaySound(frequency, 1000));
        }
        private  Tuple<string, string> GetDisplayKeyAndNote(int octave, int noteIndex)
        {
            int displayKeyAndNoteIndex = octave * 12 + noteIndex;
            var displayKeyAndNote = pianoMapping.ElementAt(displayKeyAndNoteIndex);
            if (displayKeyAndNote.Key.StartsWith("Oem") || displayKeyAndNote.Key.StartsWith("Left")
                || displayKeyAndNote.Key.StartsWith("Right"))
            {
                return Tuple.Create(OemStringMapper.Convert(displayKeyAndNote.Key), displayKeyAndNote.Value);
            }
            return Tuple.Create(displayKeyAndNote.Key, displayKeyAndNote.Value);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
