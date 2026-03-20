namespace WPF_Piano.ViewModel
{
    public class MainViewVM
    {
        public PianoButtonVM PianoButtonVM;
        public SongPlayerVM SongPlayerVM;

        public MainViewVM()
        {
            PianoButtonVM = new PianoButtonVM();
        }

    }
}
