using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Piano
{
    public class CustomPianoButton : Button
    {
        public static readonly DependencyProperty KeyLabelProperty =
            DependencyProperty.Register("KeyLabel", typeof(string), typeof(CustomPianoButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty NoteLabelProperty =
            DependencyProperty.Register("NoteLabel", typeof(string), typeof(CustomPianoButton), new PropertyMetadata(string.Empty));

        public string KeyLabel
        {
            get => (string)GetValue(KeyLabelProperty);
            set => SetValue(KeyLabelProperty, value);
        }

        public string NoteLabel
        {
            get => (string)GetValue(NoteLabelProperty);
            set => SetValue(NoteLabelProperty, value);
        }
        // Add Click event handler
        public event RoutedEventHandler CustomClick;
        protected override void OnClick()
        {
            base.OnClick();
            CustomClick?.Invoke(this, new RoutedEventArgs());
        }

    }
}
