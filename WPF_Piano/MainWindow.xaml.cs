


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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Empty;
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "MIDI files (*.mid)|*.mid|All files (*.*)|*.*";
                dialog.Title = "Select a MIDI File";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = dialog.FileName;
                    midiFile = new NAudio.Midi.MidiFile(filePath, true);
                    this.NoteControl.MidiFile = midiFile;
                    NoteFrame.ScrollToBottom();
                }
            }
            if (filePath == string.Empty) return;
          

        }

        public async void Play(object sender, RoutedEventArgs e)
        {
        
            double frameRate = 60.0;
            double frameTime = 1.0 / frameRate; // ~0.0166 seconds
            songDuration = PianoPlaySound.Instance.CalculateSongDuration(midiFile); 
         
            double startScroll = NoteFrame.ExtentHeight - NoteFrame.ViewportHeight;

         
            double scrollPixelsPerSecond = 150.0;

            IsPlaying = true;
            if (currentTime == 0)
            {
                for (double time = 0; time < songDuration; time += frameTime)
                {
                    if (IsPlaying == false) break;
                 
                    double currentScroll = startScroll - (time * scrollPixelsPerSecond);

                    if (currentScroll < 0) currentScroll = 0;

                    NoteFrame.ScrollToVerticalOffset(currentScroll);
                    currentTime = time;
                   
                    await Task.Delay(TimeSpan.FromSeconds(frameTime));
                }
            }
            else
            {
                for (double time = currentTime; time < songDuration; time += frameTime)
                {
                    if (IsPlaying == false) break;

                    double currentScroll = startScroll - (time * scrollPixelsPerSecond);
                    
                    if (currentScroll < 0) currentScroll = 0;
                    
                    NoteFrame.ScrollToVerticalOffset(currentScroll);                    
                    currentTime = time;
                    
                    await Task.Delay(TimeSpan.FromSeconds(frameTime));
                }
            }
        }
        public async void Pause(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;
        }

    }
}