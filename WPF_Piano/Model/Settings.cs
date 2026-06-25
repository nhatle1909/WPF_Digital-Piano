using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Piano.Model
{
    public class PianoKey : INotifyPropertyChanged
    {
        private string key;
        private string note;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Key
        {
            get => key;
            set
            {
                if (key != value)
                {
                    key = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Note
        {
            get => note;
            set
            {
                if (note != value)
                {
                    note = value;
                    OnPropertyChanged();
                }
            }
        }
    }
    public class PianoOctave : INotifyPropertyChanged
    {
        private string from;
        private string to;
        public string From
        {
            get => from;
            set
            {
                if (from != value)
                {
                    from = value;
                    OnPropertyChanged();
                }
            }
        }
        public string To
        {
            get => to;
            set
            {
                if (to != value)
                {
                    to = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class MiscSettings : INotifyPropertyChanged
    {
        private bool isCopyingFile;
        public bool IsCopyingFile
        {
            get => isCopyingFile;
            set
            {
                if (isCopyingFile != value)
                {
                    isCopyingFile = value;
                    OnPropertyChanged();
                }
            }
        }
       
     
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
