


using NAudio.Midi;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPF_Piano.Helper;
using WPF_Piano.ViewModel;

namespace WPF_Piano
{
    public partial class MainWindow : Window
    {


        public MainViewVM MainViewVM;
        public Storyboard storyBoard;

        public MainWindow()
        {
            InitializeComponent();
            MainViewVM = new MainViewVM(this, PianoButtonOctave);
            storyBoard = new();
            this.DataContext = MainViewVM;
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            
            this.KeyUp += UnhighlightKey;
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            MainViewVM.SongPlayerVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "LoadedMidi")
                {
                    RemoveSong();
                    NoteFrame.ScrollToBottom();
                    NoteFrame.UpdateLayout();

                }

            };

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
        public void Play_Song(object sender, RoutedEventArgs e)
        {
            if (storyBoard.Children.Count == 0)
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation
                {
                    From = NoteFrame.ScrollableHeight, // Start at the bottom
                    To = 0,          // Move to the top
                    Duration = TimeSpan.FromSeconds(MainViewVM.SongPlayerVM.TotalDuration),

                };
               
                Storyboard.SetTarget(verticalAnimation, NoteFrame);

                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
                storyBoard.Children.Add(verticalAnimation);
                storyBoard.Begin(NoteControl,isControllable:true);
                
          
                return;
            }
            storyBoard.Resume();
         
               
        }
        public void Pause_Song(object sender, RoutedEventArgs e)
        {
            storyBoard.Pause();
        }
        public void RemoveSong()
        {
            if (storyBoard.Children.Count == 0) return;

            storyBoard.Stop(NoteFrame);
            storyBoard.Remove(NoteFrame);
            storyBoard = null;
            storyBoard = new();

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

        private void Tabbar_Click(object sender, RoutedEventArgs e)
        {
            if (sender == SettingsButton)
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            }
        }
    }
}