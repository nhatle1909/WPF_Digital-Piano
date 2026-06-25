using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_Piano.ViewModel
{
    public class MainViewVM
    {
        public PianoButtonVM PianoButtonVM { get; set; }
        public SongPlayerVM SongPlayerVM { get; set; }
        public SongVM SongVM { get; set; } 
        public MainViewVM(FrameworkElement Fe, StackPanel PianoButtonOctave)
        {
            PianoButtonVM = new PianoButtonVM(Fe, PianoButtonOctave);
            SongPlayerVM = new SongPlayerVM();
            SongVM = new SongVM();
            SongVM.SongSelected += (song) =>
            {
                SongPlayerVM.LoadMidiFile(song.FilePath);
            };
           
        }
    }
}