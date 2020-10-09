using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeLib;

namespace TESV_EspEquipmentGenerator
{
    public struct PartitionFlag {
        Partitions partitionCache;
        public Partitions Partition => partitionCache;
        public bool IsEnable;
        public PartitionFlag(Partitions partition,bool isEnable)
        {
            partitionCache = partition;
            IsEnable = isEnable;
        }
    }
    public class BipedBodyTemplate : RecordObject
    {

        //public PartitionFlags[] FirstPersonFlags {
        //    get { handle.GetElement(); }
        //    set { property = value; } 
        //}
    }
}
