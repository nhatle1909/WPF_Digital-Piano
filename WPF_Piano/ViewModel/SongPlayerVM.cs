using Microsoft.Win32;
using NAudio.Midi;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPF_Piano.Helper;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class SongPlayerVM : INotifyPropertyChanged
    {
        private SongPlayer _model = new();
   
        private double _width;
        private double _scrollOffset;
        private bool isFinished = false;
        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }
        public bool IsFinished
        {
            get => isFinished;
            set { isFinished = value; OnPropertyChanged(nameof(IsFinished)); }
        }
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
            PauseCommand = new RelayCommand(_ => Pause());
            SkipCommand = new RelayCommand(_ => Skip());
        }

        private void LoadMidiFile()
        {
            OpenFileDialog dialog = new() { Filter = "MIDI files (*.mid)|*.mid" };
            if (dialog.ShowDialog() == true)
            {
                TotalDuration = PianoPlaySound.Instance.CalculateSongDuration(LoadedMidi);
                LoadedMidi = new MidiFile(dialog.FileName, true);
            
                IsPlaying = false;
                isFinished = false;
                CurrentTime = 0;
              
            }
        }
        public void LoadMidiFile(string path)
        {
            if (File.Exists(path))
            {
                TotalDuration = PianoPlaySound.Instance.CalculateSongDuration(new MidiFile(path, true));
                LoadedMidi = new MidiFile(path, true);

                IsPlaying = false;
                isFinished = false;
                CurrentTime = 0;
             
            }    
        
        }
        private async void Play()
        {
            if (LoadedMidi == null || IsPlaying) return;

            IsPlaying = true;
            isFinished = false;


            double frameRate = 60.0;
            double pixelsPerSecond = 200;
            double frameTime = 1.0 / frameRate;

            await Task.Run(async () =>
            {
                while (IsPlaying && CurrentTime < TotalDuration)
                {
                    CurrentTime += frameTime;
                    await Task.Delay(TimeSpan.FromSeconds(frameTime));

                    if (CurrentTime >= TotalDuration)
                    {
                        IsPlaying = false;
                        await Task.Delay(500);
                        isFinished = true;

                        // Reset on UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CurrentTime = 0;
                        });
                    }
                }
            });

        }
        private async void Pause()
        {
            IsPlaying = false;
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