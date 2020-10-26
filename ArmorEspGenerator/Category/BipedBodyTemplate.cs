using System;
using XeLib;
//using Yu5h1Tools.WPFExtension;

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
        public static string Signature => "Biped Body Template";
        public override string signature => Signature;
        public override Handle handle {
            get {
                return parent.HasElement("BODT") ? parent.GetElement("BODT") : base.handle;
            }
            protected set => base.handle = value;
        }
        public override string DisplayName {
            get {
                if (handle == null) return signature;
                return base.DisplayName;
            }
        }
        public BipedBodyTemplate(Handle Parent) : base(Parent){}

        public static string FirstPersonFlagsKey => @"First Person Flags";
        public PartitionFlag[] FirstPersonFlags
        {
            get => GetPartitionFlags(handle.GetValue(FirstPersonFlagsKey));
            set {
                PrepareHandle();
                handle.SetValue(FirstPersonFlagsKey, value.ToValue());
            }
        }
        public static string ArmorTypeKey => @"Armor Type";
        public string ArmorType {
            get => handle.GetElement(ArmorTypeKey).GetValue();
            set {
                PrepareHandle();
                handle.SetValue(ArmorTypeKey, value);
            } 
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

        public void SetPartitionsFromNif(string nifFile)
        {
            SetPartitionFlags(PartitionsUtil.ConvertIndicesToBodyParts(Plugin.
                                GetBodyPartsIndicesFromNif(nifFile)).
                                BSDismemberBodyPartsToPartitions());
        }
    }
}
