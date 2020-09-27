using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public static class StringEx
    {
        public static string[] GetLines(this string txt) => txt.Split('\n');
        public static bool Contains(this string txt, params string[] args)
        {
            foreach (var arg in args) if (txt.ToLower().Contains(arg.ToLower())) return true;
            return false;
        }
        public static string MakeValidEditorID(this string txt)
        {
            txt = txt.Split('_').Select(d => d.FirstCharToUpper()).Join();
            txt = txt.Split(' ').Select(d => d.FirstCharToUpper()).Join();
            return new Regex("[^a-zA-Z0-9]").Replace(txt, "");
        }
        public static string TrimEndNumber(this string txt)
          => txt.TrimEnd(new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
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

        public static string FirstCharToUpper(this string txt)
          =>  txt == string.Empty || txt == null ? "" : txt[0].ToString().ToUpper() + txt.Substring(1);

        public static string NameWithOutExtension(this string txt) => Path.GetFileNameWithoutExtension(txt);
        public static string MakeArmorAddonName(this string txt, string suffix) => txt.RemoveSuffixFrom("AA").TrimEndNumber() + suffix + "AA";
    }
}
