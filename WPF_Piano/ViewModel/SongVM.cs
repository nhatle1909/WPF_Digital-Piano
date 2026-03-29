using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Piano.Model;
namespace WPF_Piano.ViewModel
{
    public class SongVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public event Action<Song>? SongSelected;
        private Song? _selectedSong;

        public Song? SelectedSong
        {
            get => _selectedSong;
            set
            {
                _selectedSong = value;
                OnPropertyChanged(nameof(SelectedSong));

                if (_selectedSong != null)
                {
                    SongSelected?.Invoke(_selectedSong);
                }
            }
        }
        private string path;
        public ICommand LoadSongCommand;
        public ICommand PlaySongCommand;
      
        public SongVM()
        {
            LoadSongCommand = new RelayCommand(LoadSong);
            PlaySongCommand = new RelayCommand(PlaySong);
            path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "Song");
            LoadSong(path);
        }
        private void LoadSong(object parameter)
        {
            if (parameter is string path)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                Songs.Clear();
                var files = Directory.GetFiles(path, "*.mid");
                foreach (var file in files)
                {
                    Songs.Add(new Song
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        FilePath = file,
                        Icon = "pack://application:,,,/Resources/song_icon.png",
                        Description = "A MIDI file",
                    });
                }
            }

          
        }
        private void AddSong(object parameter)
        {
            // Implement song adding logic here
        }
        private void PlaySong(object parameter)
        {
            // Implement song playing logic here
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
