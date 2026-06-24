


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


        public MainViewVM MainViewVM ;
        public Storyboard storyBoard = new();

        public MainWindow()
        {
            InitializeComponent();
            MainViewVM = new MainViewVM(this, PianoButtonOctave);
            this.DataContext = MainViewVM;
            this.KeyDown += Key_Pressed;
            this.KeyDown += HighlightKey;
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            this.KeyUp += UnhighlightKey;
            MainViewVM.SongPlayerVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "LoadedMidi")
                {
                    NoteFrame.ScrollToBottom();
                    NoteFrame.UpdateLayout();
                    storyBoard.Children.Clear();
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
                storyBoard.Children.Add(verticalAnimation);
                Storyboard.SetTarget(verticalAnimation, NoteFrame);

                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
                storyBoard.Begin();
                return;
            }
            storyBoard.Resume();
         
               
        }
        public void Pause_Song(object sender, RoutedEventArgs e)
        {
            storyBoard.Pause();
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