


using NAudio.Midi;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPF_Piano.Deprecated;
using WPF_Piano.Helper;
using WPF_Piano.ViewModel;

namespace WPF_Piano
{
    public partial class MainWindow : Window
    {


        public MainViewVM MainViewVM = new();
        MidiFile midiFile;
        double songDuration;
        double currentTime;
        bool IsPlaying = false;
        public MainWindow()
        {
            InitializeComponent();
            // Set up the key event handler for the window
            this.DataContext = MainViewVM;
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            this.KeyUp += UnhighlightKey;
            MainViewVM.SongPlayerVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ScrollOffset")
                {
                    double totalExtent = NoteFrame.ExtentHeight;
                    double viewport = NoteFrame.ViewportHeight;
               
                    NoteFrame.ScrollToVerticalOffset(totalExtent - viewport - MainViewVM.SongPlayerVM.ScrollOffset);
                }
            };
            PianoUIRender.Instance.RenderButton(this, PianoButtonOctave);
            NoteFrame.ScrollToBottom();
        }

        public void Key_Pressed(object sender, KeyEventArgs e)
        {
            MainViewVM.PianoButtonVM.PlayNote(e);
            //if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.S)
            //{
            //    MessageBox.Show("Ctrl+S pressed!");
            //}
        }


        public void HighlightKey(object sender, KeyEventArgs e)
        {
            var buttonPressed = e.Key.ToString();
           
            var button = FEHelper.FindTheButton(this, buttonPressed);
            var background = FEHelper.FindElementByName(button, "Background") as Button;
            if (background != null)
            {
                background.Background = Brushes.Yellow;
            }

        }
        public void UnhighlightKey(object sender, KeyEventArgs e)
        {
            
            var buttonPressed = e.Key.ToString();
          
            var button = FEHelper.FindTheButton(this, buttonPressed);

            if (button == null) return;

            var background = FEHelper.FindElementByName(button, "Background") as Button;
            if (background != null)
            {
                background.Background = button.Name.Contains("Black") ? Brushes.Black : Brushes.White;
            }
        }



    }
}