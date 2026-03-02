namespace WPF_Piano.Helper
{
    public static class OemStringMapper
    {
        private static readonly Dictionary<string, string> OemMap = new()
    {
        { "OemComma", "," },
        { "OemPeriod", "." },
        { "OemQuestion", "/" },
        { "OemSemicolon", ";" },
        { "OemQuotes", "'" },
        { "OemOpenBrackets", "[" },
        { "Oem6", "]" },
        { "OemBackslash", "\\" },
        { "OemMinus", "-" },
        { "OemPlus", "=" },
        { "Oemtilde", "`" },
        {"Oem1", ";" },
        {"LeftShift","LS" },
        {"RightShift","RS" },
        {"Oem5","\\" } // For some keyboards, Oem5 is backslash
    };

        public static string Convert(string keyName)
        {
            return OemMap.TryGetValue(keyName, out var value) ? value : keyName;
        }
    }
}
