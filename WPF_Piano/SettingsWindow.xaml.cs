using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF_Piano.ViewModel;

namespace WPF_Piano
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var settingsVM = new SettingsVM();
            System.Diagnostics.Debug.WriteLine($"Total keys loaded: {settingsVM.PianoKeys.Count}");
            this.DataContext = settingsVM;
        }
        public void Save_Click(object sender, RoutedEventArgs e)
        {
            // Save settings logic here
            MessageBox.Show("Settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            
            this.Close();
        }
        public void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
 

