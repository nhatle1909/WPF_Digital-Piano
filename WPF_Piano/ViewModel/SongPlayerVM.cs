using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Piano.Model;

namespace WPF_Piano.ViewModel
{
    public class SongPlayerVM
    {
        private SongPlayer _model;
        private ScrollViewer _scrollViewer;
        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }

        public SongPlayerVM()
        {
            _model = new SongPlayer();
            StartCommand = new RelayCommand(StartScroll);
        }
        public void StartScroll(object parameter)
        {
            
        }
    }
}
