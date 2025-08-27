using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Piano
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
        {"LeftShift","LS" },
        {"RightShift","RS" }
    };

        public static string Convert(string keyName)
        {
            return OemMap.TryGetValue(keyName, out var value) ? value : keyName;
        }
    }
}
