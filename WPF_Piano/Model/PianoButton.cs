namespace WPF_Piano.Model
{
    public class PianoButton
    {
        public string Note;
        public string Key;
        public float Frequency;
        public PianoButton(string note, string key, float frequency)
        {
            Note = note;
            Key = key;
            Frequency = frequency;
        }
    }
}
