using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeLib;

namespace TESV_EspEquipmentGenerator
{
    public class Keywords : RecordElement<Keywords>
    {
        public static string Signature = "KYWD";
        public override string signature => Signature;

        public static Handle[] GetHandles() => SkyrimESM.GetRecords(Signature);

        public Keywords(PluginRecords<Keywords> container, Handle target) : base(container, target)
        {
            
        }
    }
}
