using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_Piano.Helper
{
    static class FEHelper
    {
        public static T FindParent<T>(this FrameworkElement element) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(element);
            if (parent == null) return null;
            if (parent is T t) return t;
            if (parent is FrameworkElement fe) return fe.FindParent<T>();
            return null;
        }
        public static FrameworkElement FindElementByName(DependencyObject parent, string name)
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.Name == name)
                    return fe;

                var result = FindElementByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
        public static T FindElementByName<T>(this FrameworkElement element, string name) where T : FrameworkElement
        {
            if (element.Name == name && element is T t) return t;
            var count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is FrameworkElement fe)
                {
                    var result = fe.FindElementByName<T>(name);
                    if (result != null) return result;
                }
            }
            return null;
        }
        public static List<T> FindElementsByType<T>(this FrameworkElement element) where T : FrameworkElement
        {
            var list = new List<T>();
            if (element is T t) list.Add(t);
            var count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is FrameworkElement fe)
                {
                    list.AddRange(fe.FindElementsByType<T>());
                }
            }
            return list;
        }
        public static Button FindTheButton(this FrameworkElement element, string keyPressed)
        {
            // if keyname is number layout ( such as D1 ) , convert it to "1"
            if (keyPressed.StartsWith("D") && keyPressed.Length > 1 && char.IsDigit(keyPressed[1]))
            {
                keyPressed = keyPressed.Substring(1);
            }
            if (!PianoSettings.Instance.CheckKey(keyPressed)) return null;
            string note = PianoSettings.Instance.GetNote(keyPressed).Contains("#") ? $"Black{keyPressed}" : $"White{keyPressed}";
            CustomPianoButton button = FindElementByName(element, note) as CustomPianoButton;
            return button;
        }
    }
}
