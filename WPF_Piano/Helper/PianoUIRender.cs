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

        public PianoUIRender()
        {
        }
        public void RenderButton(FrameworkElement Fe, StackPanel PianoButtonOctave)
        {

            for (int i = 0; i < 4; i++)
            {
                var Grid = new Grid
                {
                    Name = $"PianoOctave{i}",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    //Margin = new Thickness(0, 0, 10, 0),
                };
                var newOctaveWhite = new StackPanel
                {
                    Name = $"WhiteButtonLayout{i}",
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var newOctaveBlack = new StackPanel
                {
                    Name = $"BlackButtonLayout{i}",
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Grid.SetColumn(newOctaveWhite, 0);
                Grid.SetColumn(newOctaveBlack, 1);
                PianoButtonOctave.Children.Add(Grid);
                Grid.Children.Add(newOctaveWhite);
                Grid.Children.Add(newOctaveBlack);
                for (int i2 = 1; i2 <= 12; i2++)
                {
                    CustomPianoButton btn;
                    var displayKeyAndNote = GetDisplayKeyAndNote(i, i2 - 1);
                    var keyName = pianoMapping.ElementAt(i2 - 1 + i * 12).Key;

                    if (i2 == 2 || i2 == 4 || i2 == 7 || i2 == 9 || i2 == 11)
                    {
                        // Add a black key
                        btn = new CustomPianoButton
                        {
                            Template = (ControlTemplate)Fe.FindResource("BlackPianoButton"),
                            MinWidth = 30,
                            MinHeight = 90,
                            Margin = ReturnBlackButtonMargin(i2),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            KeyLabel = displayKeyAndNote.Item1, // Set the key label
                            NoteLabel = displayKeyAndNote.Item2, // Set the note label
                            Name = $"Black{keyName}",
                            FontSize = 12
                        };
                        newOctaveBlack.Children.Add(btn);
                    }
                    else
                    {
                        // Add a white key
                        btn = new CustomPianoButton
                        {
                            Template = (ControlTemplate)Fe.FindResource("WhitePianoButton"),
                            MinWidth = 60,
                            MinHeight = 180,
                            Margin = new Thickness(0, 0, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            KeyLabel = displayKeyAndNote.Item1, // Set the key label
                            NoteLabel = displayKeyAndNote.Item2, // Set the note label        
                            Name = $"White{keyName}",
                            FontSize = 12,

                        };
                        newOctaveWhite.Children.Add(btn);
                    }
                }
            }
        }



        private Tuple<string, string> GetDisplayKeyAndNote(int octave, int noteIndex)
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
        public Thickness ReturnBlackButtonMargin(int index)
        {
            // Calculate the margin for black keys based on their position
            switch (index)
            {
                case 2: return new Thickness(0, 0, 0, 20); // First black key
                case 4: return new Thickness(30, 0, 0, 20); // Second black key
                case 7: return new Thickness(90, 0, 0, 20); // Third black key
                case 9: return new Thickness(30, 0, 0, 20); // Fourth black key
                case 11: return new Thickness(30, 0, 0, 20); // Default margin for other keys
                default: return new Thickness(0, 0, 0, 0); // Default margin for other keys
            }
        }
        public Tuple<HorizontalAlignment, int> ReturnWhiteTileAlignment(int index)
        {
            var align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center, 0);
            if (index == 0) return new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Left, 45);
            var modIndex = index % 12;
            switch (modIndex)
            {
                case 0: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Left, 45); break; // First black key
                case 2: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center, 30); break; // Second black key
                case 4: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Right, 45); break; // Third black key
                case 5: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Left, 45); break; // Fourth black key
                case 7: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center, 30); break; // Default margin for other keys
                case 9: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center, 30); break; // Default margin for other keys
                case 11: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Right, 45); break; // Default margin for other keys
                default: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center, 30); break; // Default margin for other keys
            }
            return align;
        }
        #region Deprecated Code
        //public void RenderNoteTileFrame(FrameworkElement Fe, StackPanel NoteTileFrame, bool isRealKey)
        //{
        //    if (NoteTileFrame == null) return;
        //    NoteTileFrame.Children.Clear();
        //    if (isRealKey)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            var Grid = new Grid
        //            {
        //                Name = $"PianoOctave{i}",
        //                HorizontalAlignment = HorizontalAlignment.Center,
        //                VerticalAlignment = VerticalAlignment.Top,
        //                //Margin = new Thickness(0, 0, 10, 0),
        //            };
        //            var newOctaveWhite = new StackPanel
        //            {
        //                Name = $"WhiteButtonLayout{i}",
        //                Orientation = Orientation.Horizontal,
        //                VerticalAlignment = VerticalAlignment.Top,
        //                HorizontalAlignment = HorizontalAlignment.Center
        //            };
        //            var newOctaveBlack = new StackPanel
        //            {
        //                Name = $"BlackButtonLayout{i}",
        //                Orientation = Orientation.Horizontal,
        //                VerticalAlignment = VerticalAlignment.Top,
        //                HorizontalAlignment = HorizontalAlignment.Center
        //            };

        //            Grid.SetColumn(newOctaveWhite, 0);
        //            Grid.SetColumn(newOctaveBlack, 1);
        //            NoteTileFrame.Children.Add(Grid);
        //            Grid.Children.Add(newOctaveWhite);
        //            Grid.Children.Add(newOctaveBlack);
        //            for (int i2 = 1; i2 <= 12; i2++)
        //            {
        //                Grid noteTile;
        //                var displayKeyAndNote = GetDisplayKeyAndNote(i, i2 - 1);
        //                var keyName = pianoMapping.ElementAt(i2 - 1 + i * 12).Key;
        //                if (i2 == 2 || i2 == 4 || i2 == 7 || i2 == 9 || i2 == 11)
        //                {
        //                    // Add a black key
        //                    noteTile = new Grid
        //                    {
        //                        MinWidth = 30,
        //                        MinHeight = 180,
        //                        Margin = new Thickness(ReturnBlackButtonMargin(i2).Left, 0, 0, 0),
        //                        HorizontalAlignment = HorizontalAlignment.Center,
        //                        VerticalAlignment = VerticalAlignment.Top,
        //                        Name = $"NoteTile{keyName}",

        //                    };

        //                    newOctaveBlack.Children.Add(noteTile);

        //                }
        //                else
        //                {
        //                    noteTile = new Grid
        //                    {
        //                        MinWidth = 60,
        //                        MinHeight = 180,
        //                        Margin = new Thickness(0, 0, 0, 0),
        //                        HorizontalAlignment = HorizontalAlignment.Center,
        //                        VerticalAlignment = VerticalAlignment.Top,
        //                        Name = $"NoteTile{keyName}",

        //                    };

        //                    newOctaveWhite.Children.Add(noteTile);

        //                }
        //            }
        //        }
        //    }
        //}
        //public void RenderNoteTile(FrameworkElement Fe, StackPanel NoteTilesFrame, NoteTileInfo noteTileInfo)
        //{

        //    // Create storyboard
        //    var keyList = pianoMapping.ToList();
        //    var keyName = pianoMapping.FirstOrDefault(x => x.Value == noteTileInfo.NoteName).Key;
        //    var frame = FindElementByName(NoteTilesFrame, $"NoteTile{keyName}") as Grid;
        //    if (frame == null) return;
        //    var noteTile = new Rectangle
        //    {
        //        RenderTransform = new TranslateTransform(),

        //        // Width: narrow for sharp notes, wider for naturals
        //        Width = noteTileInfo.NoteName.Contains("#") ? 30 : 60,

        //        // Height: scale velocity to a visible range (e.g., 0–127 → 10–100)
        //        Height = Math.Max(10, noteTileInfo.Velocity * 0.4),
        //        Stroke = Brushes.Black,
        //        StrokeThickness = 0.5,
        //        Fill = Brushes.LightGreen,


        //        VerticalAlignment = VerticalAlignment.Top,
        //        HorizontalAlignment = HorizontalAlignment.Left // or Center if needed
        //    };


        //    var animation = new DoubleAnimation
        //    {
        //        From = 0,
        //        To = frame.ActualHeight,
        //        Duration = TimeSpan.FromSeconds(5),


        //    };

        //    animation.Completed += (s, e) =>
        //    {
        //        frame.Children.Remove(noteTile);
        //        float noteFrequency = NoteValue.NoteFrequencies.ContainsKey(noteTileInfo.NoteName) ? NoteValue.NoteFrequencies[noteTileInfo.NoteName] : 0;

        //            PianoPlaySound.PlaySound(noteFrequency, 1000);
        //    };
        //    frame.Children.Add(noteTile);
        //    var renderTransform = noteTile.RenderTransform as TranslateTransform;
        //    renderTransform.BeginAnimation(TranslateTransform.YProperty, animation);

        //}
        #endregion

    }
}
