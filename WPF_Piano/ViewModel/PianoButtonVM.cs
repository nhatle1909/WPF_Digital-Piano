using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Piano;
using WPF_Piano.Helper;
using WPF_Piano.Model;

public class PianoButtonVM : INotifyPropertyChanged
{
    private readonly Dictionary<string, string> _pianoMapping = PianoSettings.Instance.PianoMapping;

    public ICommand PlayNoteCommand { get; }
    public List<PianoButton> PianoButtons { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    FrameworkElement elementProvider;
    StackPanel targetOctavePanel;
    public PianoButtonVM(FrameworkElement elementProvider, StackPanel targetOctavePanel)
    {
        PlayNoteCommand = new RelayCommand(PlayNote);

        foreach (var kvp in _pianoMapping)
        {
            float frequency = NoteValue.GetFrequency(kvp.Value);
            PianoButtons.Add(new PianoButton(kvp.Value, kvp.Key, frequency));
        }
        this.elementProvider = elementProvider;
        this.targetOctavePanel = targetOctavePanel;
        RenderButton();
        PianoSettings.Instance.MappingUpdated += RenderButton;
        PianoSettings.Instance.OctaveUpdated += RenderButton;
    }

    public void RenderButton()
    {
        if (elementProvider == null || targetOctavePanel == null) return;

        var octaveRange = PianoSettings.Instance.PianoOctave;
        if (octaveRange == null || string.IsNullOrEmpty(octaveRange.From)) return;

        int firstOctaveIndex = int.Parse(octaveRange.From.Last().ToString()) - 1;
        targetOctavePanel.Children.Clear();

        int totalKeyIndex = 0;

        for (int i = firstOctaveIndex; i < 6; i++)
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
            targetOctavePanel.Children.Add(octaveGrid);

            for (int i2 = 1; i2 <= 12; i2++)
            {
                var displayKeyAndNote = GetDisplayKeyAndNote(i, i2 - 1);

                string keyName = "Unknown";
                if (totalKeyIndex < _pianoMapping.Count)
                {
                    keyName = _pianoMapping.ElementAt(totalKeyIndex).Key;
                }

                if (new[] { 2, 4, 7, 9, 11 }.Contains(i2))
                {
                    var btn = new CustomPianoButton
                    {
                        Template = (ControlTemplate)elementProvider.FindResource("BlackPianoButton"),
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
                else
                {
                    var btn = new CustomPianoButton
                    {
                        Template = (ControlTemplate)elementProvider.FindResource("WhitePianoButton"),
                        Width = 60,
                        Height = 180,
                        KeyLabel = displayKeyAndNote.Item1,
                        NoteLabel = displayKeyAndNote.Item2,
                        Name = $"White{keyName}",
                        FontSize = 12
                    };
                    whiteLayout.Children.Add(btn);
                }

                totalKeyIndex++;
            }
        }
    }

    public void PlayNote(object parameter)
    {
        if (parameter is KeyEventArgs e)
        {
            string normalizedKey = NormalizeKeyName(e.Key.ToString());

            var pianoButton = PianoButtons.FirstOrDefault(pb => pb.Key == normalizedKey);
            if (pianoButton != null)
            {
                PlayFrequency(pianoButton.Frequency);
            }
        }
    }

    private void PlayFrequency(float frequency)
    {
        Task.Run(() => PianoPlaySound.Instance.PlaySound(frequency, 1000));
    }

    private Tuple<string, string> GetDisplayKeyAndNote(int octave, int noteIndex)
    {
        int displayKeyAndNoteIndex = octave * 12 + noteIndex;
        if (displayKeyAndNoteIndex >= _pianoMapping.Count) return Tuple.Create(string.Empty, string.Empty);

        var displayKeyAndNote = _pianoMapping.ElementAt(displayKeyAndNoteIndex);

        bool isSpecialKey = displayKeyAndNote.Key.StartsWith("Oem") ||
                            displayKeyAndNote.Key.StartsWith("Left") ||
                            displayKeyAndNote.Key.StartsWith("Right");

        string finalKeyDisplay = isSpecialKey ? OemStringMapper.Convert(displayKeyAndNote.Key) : displayKeyAndNote.Key;
        return Tuple.Create(finalKeyDisplay, displayKeyAndNote.Value);
    }

    private string NormalizeKeyName(string keyName)
    {
        if (keyName.StartsWith("D") && keyName.Length > 1 && char.IsDigit(keyName[1]))
        {
            return keyName.Substring(1);
        }
        return keyName;
    }
}