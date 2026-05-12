using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_Piano
{
    public static class ScrollViewerBehavior
    {

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new PropertyMetadata(0.0, OnVerticalOffsetChanged));
        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }
        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }
        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
        #region FinishTrigger
        public static readonly DependencyProperty FinishTriggerProperty =
            DependencyProperty.RegisterAttached(
                "FinishTrigger",
                typeof(bool),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(false, OnFinishTriggerChanged));

        public static bool GetFinishTrigger(DependencyObject obj) => (bool)obj.GetValue(FinishTriggerProperty);
        public static void SetFinishTrigger(DependencyObject obj, bool value) => obj.SetValue(FinishTriggerProperty, value);

        private static void OnFinishTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                if ((bool)e.NewValue)
                {
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && e.ExtentHeightChange > 0)
            {
                scrollViewer.ScrollToBottom();
            }
        }
        #endregion
        #region ChangeTrigger

        public static readonly DependencyProperty ChangeTriggerProperty =
            DependencyProperty.RegisterAttached(
                "ChangeTrigger",
                typeof(object),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(null, OnSignalTriggerChanged));

        public static object GetChangeTrigger(DependencyObject obj) => obj.GetValue(ChangeTriggerProperty);
        public static void SetChangeTrigger(DependencyObject obj, object value) => obj.SetValue(ChangeTriggerProperty, value);

        private static void OnSignalTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToBottom();
            }
        }
        #endregion
    }
}
