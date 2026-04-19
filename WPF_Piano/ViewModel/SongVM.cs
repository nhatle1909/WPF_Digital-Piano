using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Piano.Model;
namespace WPF_Piano.ViewModel
{
    public class SongVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        private string _path;
        private bool isCopy = true;

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
        public bool IsCopy
        {
            get => isCopy;
            set { isCopy = value; OnPropertyChanged(nameof(IsCopy)); }
        }
  
        public ICommand AddSongCommand { get; }

        public SongVM()
        {
            AddSongCommand = new RelayCommand(AddSong);
            _path = AppDomain.CurrentDomain.BaseDirectory;
            _path = Path.Combine(_path, "Song");
            LoadSong(_path);
        }
        private void LoadSong(object parameter)
        {
            if (parameter is string path)
            {
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
            OpenFileDialog dialog = new() { Filter = "MIDI files (*.mid)|*.mid" };
            if (dialog.ShowDialog() == true)
            {                 
                var newSong = new Song
                {
                    Name = Path.GetFileNameWithoutExtension(dialog.FileName),
                    FilePath = dialog.FileName,
                    Icon = "pack://application:,,,/Resources/song_icon.png",
                    Description = "A new MIDI file",
                };
                Songs.Add(newSong);
                if (isCopy)
                {
                    var sourcePath = dialog.FileName;
                    var destinationPath = Path.Combine(_path, Path.GetFileName(sourcePath));
                    File.Copy(sourcePath, destinationPath, true);
                }
            }         
        }
       
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
