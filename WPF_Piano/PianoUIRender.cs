using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WPF_Piano.Helper;
using static WPF_Piano.MainWindow;

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
                            FontSize = 12
                        }; 
                        newOctaveWhite.Children.Add(btn);       
                    }
                }
            }
        }
        public void RenderNoteTileFrame(FrameworkElement Fe, StackPanel NoteTileFrame,bool isRealKey)
        {
            if (NoteTileFrame == null) return;
            NoteTileFrame.Children.Clear();
            if (isRealKey)
            {
                for (int i = 0; i < 4; i++)
                {
                    var octaveGrid = new Grid
                    {
                        Name = $"PianoOctave{i}",
                  
                    };
                    
             
                    var whiteKeysPanel = new StackPanel
                    {
                        Name = $"WhiteButtonLayout{i}",
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Center,
        
                    };
                    
                    var blackKeysPanel = new StackPanel
                    {
                        Name = $"BlackButtonLayout{i}",
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Center,
    
                    };

                    Grid.SetColumn(whiteKeysPanel, 0);
                    Grid.SetColumn(blackKeysPanel, 1);
                    
                    octaveGrid.Children.Add(whiteKeysPanel);
                    octaveGrid.Children.Add(blackKeysPanel);
                    NoteTileFrame.Children.Add(octaveGrid);
                    
                    // Create note tiles for this octave
                    for (int i2 = 1; i2 <= 12; i2++)
                    {
                        var mappingIndex = i2 - 1 + i * 12;
                        if (mappingIndex >= pianoMapping.Count) break;
                        
                        var keyName = pianoMapping.ElementAt(mappingIndex).Key;
                        var noteName = pianoMapping.ElementAt(mappingIndex).Value;
                        var isBlackKey = noteName.Contains("#");
                        
                        Grid noteTile;
                        if (isBlackKey)
                        {
                            // Black key tile
                            noteTile = new Grid
                            {
                                Width = 25,
                                Height = 200,
                                Margin = new Thickness(ReturnBlackButtonMargin(i2).Left, 0, 0, 0),
                                Name = $"NoteTile{keyName}",
                           
                            };
                            blackKeysPanel.Children.Add(noteTile);
                        }
                        else
                        {
                            // White key tile
                            noteTile = new Grid
                            {
                                Width = 50,
                                Height = 200,
                                Margin = new Thickness(0, 0, 0, 0),
                                Name = $"NoteTile{keyName}",

                            };
                            whiteKeysPanel.Children.Add(noteTile);
                        }
                    }
                }
            }
        }
        public async void RenderNoteTile(FrameworkElement Fe, StackPanel NoteTilesFrame, NoteTileInfo noteTileInfo)
        {
            var keyName = pianoMapping.FirstOrDefault(x => x.Value == noteTileInfo.NoteName).Key;
            if (keyName == null) return;
            var frame = FindElementByName(NoteTilesFrame, $"NoteTile{keyName}") as Grid;
            if(frame == null) return;
            
            var isBlackKey = noteTileInfo.NoteName.Contains("#");
            var displayKeyName = keyName.StartsWith("Oem") || keyName.StartsWith("Left") || keyName.StartsWith("Right") ? OemStringMapper.Convert(keyName) : keyName;


            var tile = new TextBlock
            {
                Text = displayKeyName,
                RenderTransform = new TranslateTransform(),
                Width = isBlackKey ? 20 : 45, // Slightly smaller to fit better
                Height = Math.Max(15, noteTileInfo.Velocity * 0.3), // Better height scaling
                Background = Brushes.Black,

                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = isBlackKey ? 8 : 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,

            };
            // Enhanced animation with easing
            var animation = new DoubleAnimation
            {
                From = 0, // Start from above the frame
                To = frame.Height, // Drop to bottom of frame
                Duration = TimeSpan.FromMilliseconds(5000), // Slightly faster
               
            };
            
            // Fade out animation
            var fadeAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.5),
                BeginTime = TimeSpan.FromSeconds(3.5) // Start fading 0.5 seconds before completion
            };
            
            animation.Completed += (s, e) =>
            {
                // Remove the tile after animation completes
                if (frame != null && frame.Children.Contains(tile))
                {
                    frame.Children.Remove(tile);
                }
            };
            
            frame.Children.Add(tile);
            var renderTransform = tile.RenderTransform as TranslateTransform;

            // Start both animations
            renderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            //tile.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
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
        public Tuple<HorizontalAlignment,int> ReturnWhiteTileAlignment(int index)
        {
            var align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center,0);
            if (index == 0 ) return new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Left,45);
            var modIndex = index % 12;
            switch (modIndex)
            {
                case 0: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Left,45); break; // First black key
                case 2: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center,30); break; // Second black key
                case 4: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Right,45); break; // Third black key
                case 5: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Left,45); break; // Fourth black key
                case 7: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center,30); break; // Default margin for other keys
                case 9: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center,30); break; // Default margin for other keys
                case 11: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Right,45); break; // Default margin for other keys
                default: align = new Tuple<HorizontalAlignment, int>(HorizontalAlignment.Center,30); break; // Default margin for other keys
            }
            return align;
        }
    }
}
