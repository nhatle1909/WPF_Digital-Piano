using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Piano.Helper
{
    static class FrameworkElementHelper
    {
        public static T FindParent<T>(this System.Windows.FrameworkElement element) where T : System.Windows.FrameworkElement
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(element);
            if (parent == null) return null;
            if (parent is T t) return t;
            if (parent is System.Windows.FrameworkElement fe) return fe.FindParent<T>();
            return null;
        }
        public static T FindElementByName<T>(this System.Windows.FrameworkElement element, string name) where T : System.Windows.FrameworkElement
        {
            if (element.Name == name && element is T t) return t;
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
                if (child is System.Windows.FrameworkElement fe)
                {
                    var result = fe.FindElementByName<T>(name);
                    if (result != null) return result;
                }
            }
            return null;
        }
        public static List<T> FindElementsByType<T>(this System.Windows.FrameworkElement element) where T : System.Windows.FrameworkElement
        {
            var list = new List<T>();
            if (element is T t) list.Add(t);
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
                if (child is System.Windows.FrameworkElement fe)
                {
                    list.AddRange(fe.FindElementsByType<T>());
                }
            }
            return list;
        }
    }
}
