using System.Linq;
using System.Collections.Generic;

namespace TESV_EspEquipmentGenerator
{
    public static class IEnumerableEx
    {
        public static string ToStringWithNumber<T>(this IEnumerable<T> enumerable, System.Func<T, string> getinfo = null, string NumSuffix = ".", string Separator = "\n") {            
            var list = enumerable.ToList();
            string result = "";
            for (int i = 0; i < list.Count; i++)
                result += i.ToString() + NumSuffix + (getinfo == null ? list[i].ToString() : getinfo(list[i])) + Separator;
            return result;
        }
        public static int FindIndex<T>(this IEnumerable<T> enumerable, T target)
        {
            var list = enumerable.ToList();
            return list.FindIndex( d => d.Equals(target));
        }
    }
}
