using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_Piano.Helper;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class SettingsVM : INotifyPropertyChanged
    {
        private PianoOctave pianoOctave { get; set; } = new();
        public PianoOctave PianoOctave
        {
            get => pianoOctave;
            set
            {
                pianoOctave = value;
                OnPropertyChanged(nameof(PianoOctave));
            }
        }
        private List<PianoKey> pianoKeys { get; set; } = new();
        public List<PianoKey> PianoKeys
        {
            get => pianoKeys;
            set
            {
                pianoKeys = value;
                OnPropertyChanged(nameof(PianoKeys));
            } 
        }
        private PianoKey SelectedKey { get; set; }
        public PianoKey SelectedPianoKey
        {
            get => SelectedKey;
            set
            {
                SelectedKey = value;
                OnPropertyChanged(nameof(SelectedPianoKey));
            }
        }
        private MiscSettings miscSettings { get; set; } = new();
        public MiscSettings MiscSettings
        {
            get => miscSettings;
            set
            {
                miscSettings = value;
                OnPropertyChanged(nameof(MiscSettings));
            }
        }
        public SettingsVM()
        {
            var keysConfig = PianoSettings.Instance.GetPianoMapping();
           
            for (int keyIndex = 0; keyIndex < keysConfig.Count; keyIndex++)
            {
                keysConfig[keyIndex].Key = OemStringMapper.Convert(keysConfig[keyIndex].Key);
                pianoKeys.Add(keysConfig[keyIndex]);
            }
            pianoOctave = PianoSettings.Instance.GetOctaveRange();
            miscSettings = PianoSettings.Instance.GetMiscSettings();
        }
        public void UpdateKey(string key)
        {
            var convertedKey = OemStringMapper.Convert(key);
            pianoKeys.FirstOrDefault(p => p == SelectedKey).Key = convertedKey;
           
        }
        public void UpdateOctave(string from, string to)
        {
            pianoOctave.From = from;
            pianoOctave.To = to;
        }
        public void SaveSettings()
        {
            var updatedMapping = pianoKeys.ToDictionary(k => OemStringMapper.Normalize(k.Key), k => k.Note);
            PianoSettings.Instance
                .UpdatePiano()
                .UpdateOctave(pianoOctave)
                .UpdateMapping(updatedMapping)
                .UpdateMiscSettings(miscSettings)
                .SaveConfig();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
      
    }
}
