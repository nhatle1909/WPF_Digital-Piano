using Microsoft.Win32;
using NAudio.Midi;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WPF_Piano.Helper;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class SongPlayerVM : INotifyPropertyChanged
    {
        private SongPlayer _model = new();
        private double _scrollOffset;
  
        public bool IsPlaying
        {
            get => _model.IsPlaying;
            set { _model.IsPlaying = value; OnPropertyChanged(nameof(IsPlaying)); }
        }
        public double CurrentTime
        {
            get => _model.CurrentTime;
            set { _model.CurrentTime = value; OnPropertyChanged(nameof(CurrentTime)); OnPropertyChanged(nameof(TimeDisplayText)); }
        }

        public double TotalDuration
        {
            get => _model.TotalDuration;
            set { _model.TotalDuration = value; OnPropertyChanged(nameof(TotalDuration)); OnPropertyChanged(nameof(TimeDisplayText)); }
        }
        public double ScrollOffset
        {
            get => _scrollOffset;
            set { _scrollOffset = value; OnPropertyChanged(nameof(ScrollOffset)); }
        }

        public string TimeDisplayText =>
            $"{FormatTime(CurrentTime)} / {FormatTime(TotalDuration)}";

        public MidiFile? LoadedMidi
        {
            get => _model.CurrentMidiFile;
            set { _model.CurrentMidiFile = value; OnPropertyChanged(nameof(LoadedMidi)); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand SkipCommand { get; }

        public SongPlayerVM()
        {
            LoadCommand = new RelayCommand(_ => LoadMidiFile());
            PlayCommand = new RelayCommand(_ => Play());
            PauseCommand = new RelayCommand(_ => _model.IsPlaying = false);
            SkipCommand = new RelayCommand(_ => Skip());
        }

        private void LoadMidiFile()
        {
            OpenFileDialog dialog = new() { Filter = "MIDI files (*.mid)|*.mid" };
            if (dialog.ShowDialog() == true)
            {
                LoadedMidi = new MidiFile(dialog.FileName, true);
                TotalDuration = PianoPlaySound.Instance.CalculateSongDuration(LoadedMidi);
                CurrentTime = 0;
            }
        }
        public void LoadMidiFile(string path)
        {
            if (File.Exists(path))
            {
                
                LoadedMidi = new MidiFile(path, true);
                TotalDuration = PianoPlaySound.Instance.CalculateSongDuration(LoadedMidi);
                CurrentTime = 0;
               
            }
        }
        private async void Play()
        {
            if (LoadedMidi == null || IsPlaying) return;

            IsPlaying = true;
            double frameRate = 120.0;
            double frameTime = 1.0 / frameRate;
            double pixelsPerSecond = 150.0;
            while (IsPlaying && CurrentTime < TotalDuration)
            {
                CurrentTime += frameTime;
                double newOffset = CurrentTime * pixelsPerSecond;
                ScrollOffset = newOffset;
                await Task.Delay(TimeSpan.FromSeconds(frameTime));
                if (CurrentTime >= TotalDuration)
                {
                  
                        IsPlaying = false;
                 
                }
            }
        }
        private async void Skip()
        {

        }

        private string FormatTime(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}