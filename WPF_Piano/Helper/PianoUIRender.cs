using System.Windows;
using System.Windows.Controls;
using WPF_Piano.Helper;

namespace WPF_Piano.Deprecated
{
    public class PianoUIRender
    {
        private static PianoUIRender _instance;
        public static PianoUIRender Instance
        {
            get
            {
                if (_instance == null) _instance = new PianoUIRender();
                return _instance;
            }
        }
        private readonly Dictionary<string, string> pianoMapping = PianoSettings.Instance.PianoMapping;

        public void RenderButton(FrameworkElement Fe, StackPanel PianoButtonOctave)
        {
            
            PianoButtonOctave.Children.Clear();
            for (int i = 0; i < 6; i++)
            {
                var octaveGrid = new Grid
                {
                    Name = $"PianoOctave{i}",
                    Width = 420,
                    Height = 180,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var whiteLayout = new StackPanel { Orientation = Orientation.Horizontal };
                var blackLayout = new Canvas { IsHitTestVisible = false };

                octaveGrid.Children.Add(whiteLayout);
                octaveGrid.Children.Add(blackLayout);
                PianoButtonOctave.Children.Add(octaveGrid);

                for (int i2 = 1; i2 <= 12; i2++)
                {
                    var displayKeyAndNote = GetDisplayKeyAndNote(i, i2 - 1);
                    var keyName = pianoMapping.ElementAt(i2 - 1 + i * 12).Key;

                    if (new[] { 2, 4, 7, 9, 11 }.Contains(i2)) // Black Keys
                    {
                        var btn = new CustomPianoButton
                        {
                            Template = (ControlTemplate)Fe.FindResource("BlackPianoButton"),
                            Width = 30,
                            Height = 110,
                            KeyLabel = displayKeyAndNote.Item1,
                            NoteLabel = displayKeyAndNote.Item2,
                            Name = $"Black{keyName}",
                            FontSize = 10
                        };

                     
                        double leftPos = i2 switch { 2 => 45, 4 => 105, 7 => 225, 9 => 285, 11 => 345, _ => 0 };
                        Canvas.SetLeft(btn, leftPos);
                        blackLayout.Children.Add(btn);
                    }
                    else // White Keys
                    {
                        var btn = new CustomPianoButton
                        {
                            Template = (ControlTemplate)Fe.FindResource("WhitePianoButton"),
                            Width = 60,
                            Height = 180,
                            KeyLabel = displayKeyAndNote.Item1,
                            NoteLabel = displayKeyAndNote.Item2,
                            Name = $"White{keyName}",
                            FontSize = 12
                        };
                        whiteLayout.Children.Add(btn);
                    }
                }
            }
        }

        public Tuple<string, string> GetDisplayKeyAndNote(int octave, int noteIndex)
        {
            int displayKeyAndNoteIndex = octave * 12 + noteIndex;
            var displayKeyAndNote = pianoMapping.ElementAt(displayKeyAndNoteIndex);
            if (displayKeyAndNote.Key.StartsWith("Oem") || displayKeyAndNote.Key.StartsWith("Left")
                || displayKeyAndNote.Key.StartsWith("Right"))
            {
                return Tuple.Create(OemStringMapper.Convert(displayKeyAndNote.Key), displayKeyAndNote.Value);
            }
            return Tuple.Create(displayKeyAndNote.Key, displayKeyAndNote.Value);
        }
        #region Deprecated 
        // This method is no longer used due to the new Canvas-based layout for black keys, which allows for precise positioning without needing margin adjustments.
        //public Thickness ReturnBlackButtonMargin(int index)
        //{
        //    // Calculate the margin for black keys based on their position
        //    switch (index)
        //    {
        //        case 2: return new Thickness(0, 0, 0, 20); // First black key
        //        case 4: return new Thickness(30, 0, 0, 20); // Second black key
        //        case 7: return new Thickness(90, 0, 0, 20); // Third black key
        //        case 9: return new Thickness(30, 0, 0, 20); // Fourth black key
        //        case 11: return new Thickness(30, 0, 0, 20); // Fifth black key
        //        default: return new Thickness(0, 0, 0, 0); // Default margin for other keys
        //    }
        //}
        //public void RenderButton(FrameworkElement Fe, StackPanel PianoButtonOctave)
        //{

        //    for (int i = 0; i < 6; i++)
        //    {
        //        var Grid = new Grid
        //        {
        //            Name = $"PianoOctave{i}",
        //            HorizontalAlignment = HorizontalAlignment.Stretch,
        //            VerticalAlignment = VerticalAlignment.Stretch,
        //            //Margin = new Thickness(0, 0, 10, 0),
        //        };
        //        var newOctaveWhite = new StackPanel
        //        {
        //            Name = $"WhiteButtonLayout{i}",
        //            Orientation = Orientation.Horizontal,
        //            VerticalAlignment = VerticalAlignment.Center,
        //            HorizontalAlignment = HorizontalAlignment.Center
        //        };
        //        var newOctaveBlack = new StackPanel
        //        {
        //            Name = $"BlackButtonLayout{i}",
        //            Orientation = Orientation.Horizontal,
        //            VerticalAlignment = VerticalAlignment.Top,
        //            HorizontalAlignment = HorizontalAlignment.Center

        //        };

        //        Grid.SetColumn(newOctaveWhite, 0);
        //        Grid.SetColumn(newOctaveBlack, 1);
        //        PianoButtonOctave.Children.Add(Grid);
        //        Grid.Children.Add(newOctaveWhite);
        //        Grid.Children.Add(newOctaveBlack);
        //        for (int i2 = 1; i2 <= 12; i2++)
        //        {
        //            CustomPianoButton btn;
        //            var displayKeyAndNote = GetDisplayKeyAndNote(i, i2 - 1);
        //            var keyName = pianoMapping.ElementAt(i2 - 1 + i * 12).Key;

        //            if (i2 == 2 || i2 == 4 || i2 == 7 || i2 == 9 || i2 == 11)
        //            {
        //                // Add a black key
        //                btn = new CustomPianoButton
        //                {
        //                    Template = (ControlTemplate)Fe.FindResource("BlackPianoButton"),
        //                    MinWidth = 30,
        //                    MinHeight = 90,
        //                    Margin = ReturnBlackButtonMargin(i2),
        //                    HorizontalAlignment = HorizontalAlignment.Center,
        //                    VerticalAlignment = VerticalAlignment.Center,
        //                    KeyLabel = displayKeyAndNote.Item1, // Set the key label
        //                    NoteLabel = displayKeyAndNote.Item2, // Set the note label
        //                    Name = $"Black{keyName}",
        //                    FontSize = 12
        //                };
        //                newOctaveBlack.Children.Add(btn);
        //            }
        //            else
        //            {
        //                // Add a white key
        //                btn = new CustomPianoButton
        //                {
        //                    Template = (ControlTemplate)Fe.FindResource("WhitePianoButton"),
        //                    MinWidth = 60,
        //                    MinHeight = 180,
        //                    Margin = new Thickness(0, 0, 0, 0),
        //                    HorizontalAlignment = HorizontalAlignment.Center,
        //                    VerticalAlignment = VerticalAlignment.Center,
        //                    KeyLabel = displayKeyAndNote.Item1, // Set the key label
        //                    NoteLabel = displayKeyAndNote.Item2, // Set the note label        
        //                    Name = $"White{keyName}",
        //                    FontSize = 12,

        //                };
        //                newOctaveWhite.Children.Add(btn);
        //            }
        //        }
        //    }
        //}
        #endregion

    }
}
