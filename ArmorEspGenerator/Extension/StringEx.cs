using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public static class StringEx
    {
        public static string RemovePrefixTo(this string txt, params string[] filter)
        {
            foreach (var item in filter)
            {
                var lastIndexOf = txt.LastIndexOf(item);
                if (lastIndexOf > -1) txt = txt.Remove(0, lastIndexOf + 1);
            }
            return txt;
        }
        public static string RemovePrefixUtilEmpty(this string txt, string pattern)
        {
            if (pattern == "") return txt;
            while (txt.StartsWith(pattern)) txt = txt.Remove(0, 1);
            return txt;
        }
        public static string ReplaceUtilEmpty(this string txt, string pattern, string newValue = "")
        {
            var IgnoreCase = RegexOptions.IgnoreCase;
            while (Regex.IsMatch(txt, pattern, IgnoreCase)) txt = Regex.Replace(txt, pattern, newValue, IgnoreCase);
            return txt;
        }
        public static string ReplaceRepeatFolderUtilEmpty(this string txt, string folder)
        {
            string pattern = folder + @"\\" + folder;
            return txt.ReplaceUtilEmpty(pattern, folder);
        }

        public static string CombineNoleadSlash(this string txt, string val)
            => Path.Combine(txt, val.RemovePrefixUtilEmpty(Path.DirectorySeparatorChar.ToString()));

    }
}
