using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Piano
{
    public static class ScrollViewerBehavior
    {
       
       public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(ScrollViewerBehavior),
            new PropertyMetadata(false, OnAutoScrollChanged));
       public static readonly DependencyProperty ChangeTriggerProperty =
        DependencyProperty.RegisterAttached("ChangeTrigger",typeof(object), typeof(ScrollViewerBehavior),
        new PropertyMetadata(null, OnSignalTriggerChanged));

       
        public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);
        public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);

        public static void SetChangeTrigger(DependencyObject obj, object value)
        => obj.SetValue(ChangeTriggerProperty, value);

        public static object GetChangeTrigger(DependencyObject obj)
            => obj.GetValue(ChangeTriggerProperty);
        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                else
                    scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Only scroll to bottom if the "extent" (height of content) increased
            if (e.ExtentHeightChange > 0)
            {
                var scrollViewer = (ScrollViewer)sender;
                scrollViewer.ScrollToBottom();
            }
        }
        private static void OnSignalTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                // Logic to run when the Model changes
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
