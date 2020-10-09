using System;
using XeLib;

namespace TESV_EspEquipmentGenerator
{
    public struct PartitionFlag {
        Partitions partitionCache;
        public Partitions Partition => partitionCache;
        public int index => (int)Partition;
        public bool IsEnable;
        public PartitionFlag(Partitions partition,bool isEnable)
        {
            partitionCache = partition;
            IsEnable = isEnable;
        }
        public override string ToString() => index.ToString() + " - " + Partition.ToString();
    }
    public class BipedBodyTemplate : RecordObject
    {
        public string Key => @"BOD2";
        public string FirstPersonFlagsKey => @"First Person Flags";

        public BipedBodyTemplate(Handle Parent,Handle bod2) : base(Parent,bod2)
        {

        }

        public PartitionFlag[] FirstPersonFlags
        {
            get => GetPartitionFlags(handle.GetValue(FirstPersonFlagsKey));
            set => handle.SetValue(FirstPersonFlagsKey, value.ToValue()); 
        }
        public static PartitionFlag[] GetPartitionFlags(string FlagsValue = "")
        {
            var partitions = (Partitions[])Enum.GetValues(typeof(Partitions));
            var results = new PartitionFlag[partitions.Length];
            for (int i = 0; i < partitions.Length; i++) {
                bool flag = i < FlagsValue.Length ? FlagsValue[i] == '1' : false;
                results[i] = new PartitionFlag(partitions[i], flag);
            } 
            return results;
        }
        public void SetPartitionFlags(Partitions[] partitions)
        {
            FirstPersonFlags = partitions.ToPartitionFlags();
        }
    }
}
