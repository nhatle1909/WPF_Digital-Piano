using System.ComponentModel;
using System.Windows.Input;
using WPF_Piano.Helper;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class PianoButtonVM : INotifyPropertyChanged
    {
        public ICommand PlayNoteCommand;
        public List<PianoButton> PianoButtons { get; set; } = new List<PianoButton>();

        public PianoButtonVM()
        {
            PlayNoteCommand = new RelayCommand(PlayNote);
            // Initialize piano buttons with their corresponding keys and frequencies
            foreach (var key in PianoSettings.Instance.PianoMapping.Keys)
            {
                string note = PianoSettings.Instance.PianoMapping[key];
                float frequency = NoteValue.GetFrequency(note);
                PianoButtons.Add(new PianoButton(note, key, frequency));
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
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
