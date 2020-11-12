using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeLib;

namespace TESV_EspEquipmentGenerator
{
    public class Keywords : RecordArrays<Handle>
    {
        public static string Signature => "KWDA";
        public override string signature => Signature;

        public Keywords(Handle Parent) : base(Parent)
        {
            var elements = handle.GetElements();
            if (handle != null && elements.Length > 0) AddRange(elements);
        }
        public void Add(params string[] IDs) {
            foreach (var id in IDs)
            {
               var found = Plugin.FindRecords(id, true);
                if (found.Length > 0) Add(found[0]);
            }
        }
    }
}
