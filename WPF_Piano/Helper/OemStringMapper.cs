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
            { "Oem3", "`" },
            {"Oem1", ";" },
            {"LeftShift","LS" },
            {"RightShift","RS" },
            {"Oem5","\\" } // For some keyboards, Oem5 is backslash
        };
        private static readonly Dictionary<string, string> NormalizationMap = new()
        {
            { ",", "OemComma" },
            { ".", "OemPeriod" },
            { "/", "OemQuestion" },
            { ";", "OemSemicolon" },
            { "'", "OemQuotes" },
            { "[", "OemOpenBrackets" },
            { "]", "Oem6" },
            { "\\", "OemBackslash" }, 
            { "-", "OemMinus" },
            { "=", "OemPlus" },
            { "`", "Oem3" },

            { "LS", "LeftShift" },
            { "RS", "RightShift" }       
        };

        public static string Convert(string keyName)
        {
            return OemMap.TryGetValue(keyName, out var value) ? value : keyName;
        }
        public static string Normalize(string keyName)
        {
            return NormalizationMap.TryGetValue(keyName, out var value) ? value : keyName;
        }
    }
}
