using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Piano
{
    public class PianoUIRender
    {
        private Dictionary<string, string> pianoMapping;
        private PianoSettings pianoButtonSettings;
        public PianoUIRender() 
        {
            pianoButtonSettings = new PianoSettings();
            pianoMapping = pianoButtonSettings.ReturnPianoMapping();
        }
        public void RenderButton(FrameworkElement Fe, StackPanel PianoButtonOctave)
        {
            for (int i = 0; i < 4; i++)
            {
                // Add the new column to the WhiteButtonLayout
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
                    var keyAndNote = GetKeyAndNote(i, i2 - 1);
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
                            KeyLabel = keyAndNote.Item1, // Set the key label
                            NoteLabel = keyAndNote.Item2, // Set the note label
                                                          //Name = keyAndNote.Item1.ToString() // Set the name for identification
                            
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
                            KeyLabel = keyAndNote.Item1, // Set the key label
                            NoteLabel = keyAndNote.Item2, // Set the note label

                        };
                        newOctaveWhite.Children.Add(btn);
                    }
                }
            }

        }
        private Tuple<string, string> GetKeyAndNote(int octave, int noteIndex)
        {
            int keyAndNoteIndex = octave * 12 + noteIndex;
            var keyAndNote = pianoMapping.ElementAt(keyAndNoteIndex);
            if (keyAndNote.Key.StartsWith("Oem") || keyAndNote.Key.StartsWith("Left")
                || keyAndNote.Key.StartsWith("Right"))
            {
                return Tuple.Create(OemStringMapper.Convert(keyAndNote.Key), keyAndNote.Value);
            }
            return Tuple.Create(keyAndNote.Key, keyAndNote.Value);
        }
        private Thickness ReturnBlackButtonMargin(int index)
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
    }
}
