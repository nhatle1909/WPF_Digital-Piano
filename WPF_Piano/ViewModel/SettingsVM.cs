using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Piano.Helper;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class SettingsVM : INotifyPropertyChanged
    {
        public ObservableCollection<PianoKey> PianoKeys { get; set; } = new ();
        public SettingsVM()
        {
            var pianoKeys = PianoSettings.Instance.GetPianoMapping();
            foreach (var key in pianoKeys)
            {
                PianoKeys.Add(key);
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
      
    }
}
