using System.ComponentModel;
using System.Windows.Input;
namespace WPF_Piano.ViewModel
{
    public class SongVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand LoadSongCommand;
        public ICommand PlaySongCommand;
        public SongVM()
        {
            LoadSongCommand = new RelayCommand(LoadSong);
            PlaySongCommand = new RelayCommand(_ => PlaySong());
        }
        private void LoadSong(object parameter)
        {
            // Implement song loading logic here
        }
        private void PlaySong()
        {
            // Implement song playing logic here
        }
    }
}
