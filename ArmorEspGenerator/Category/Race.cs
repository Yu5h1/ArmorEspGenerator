using XeLib;
using XeLib.API;

namespace TESV_EspEquipmentGenerator
{
    public class Race : RecordElement<Race>
    {
        public const string Signature = "RACE";
        public override string signature => Signature;
        public Race(PluginRecords<Race> container, Handle target) : base(container, target)
        {
        }
    }
    public class AdditionalRaces : RecordArrays<Handle> {
        public const string Signature = "Additional Races";
        public override string signature => Signature;
        public AdditionalRaces(Handle Parent) : base(Parent)
        {
            if (handle != null) {
                foreach (var item in parent.GetArrayItems(signature))
                {
                    Add(item);
                }
            }
        }
        public new void Add(Handle handle) {
            parent.AddArrayItem(signature, "", handle.GetFormID());
        }

    }
}
