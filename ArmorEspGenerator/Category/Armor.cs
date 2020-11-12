using System.Linq;
using XeLib.API;
using XeLib;

namespace TESV_EspEquipmentGenerator
{

    public class Armor : RecordElement<Armor>
    {
        public static readonly string[] ArmorTypes = new string[]
        {
            "Clothing",
            "Heavy Armor",
            "Light Armor"
        };

        public const string Signature = "ARMO";
        public override string signature => Signature;
        public string FULLName { get => GetValue(FullNameKEY); set => SetValue(FullNameKEY, value); }
        public BipedBodyTemplate bipedBodyTemplate;
        public ModelAlternateTextures MaleWorldModel;
        public ModelAlternateTextures FemaleWorldModel;
        public Armatures armatures;
        public Keywords keywords;
        public static string ValueKey = @"DATA\Value";
        public static string WeightKey = @"DATA\Weight";
        public int Value {
            get => handle.GetInteger(ValueKey);
            set => handle.SetInteger(ValueKey, value);
        }
        public double Weight
        {
            get => handle.GetFloat(WeightKey);
            set => handle.SetFloat(WeightKey, value);
        }
        public static string RatingKey = "DNAM";
        public double Rating
        {
            get => handle.GetFloat(RatingKey);
            set => handle.SetFloat(RatingKey, value);
        }
        public string Race
        {
            get => handle.GetElement(RaceKey).GetValue();
            set => handle.SetValue(RaceKey, value);
        }
        public Armor(Handle target, PluginRecords<Armor> container = null) : base(container, target) {
            armatures = new Armatures(this);
            bipedBodyTemplate = new BipedBodyTemplate(target);
            MaleWorldModel = new ModelAlternateTextures(target, true);
            FemaleWorldModel = new ModelAlternateTextures(target, false);
            keywords = new Keywords(target);
        }
        public static Armor Create(PluginRecords<Armor> container, Handle handle) {
            if (handle.CompareSignature<Armor>()) return new Armor(handle, container);
            else return null;
        }
        public void SetModelAssets(bool maleOrfemale,EquipmentAsset asset) {
            if (asset == null) return;
            if (asset.ItemModel == "") return;
            if (maleOrfemale) MaleWorldModel.Model = asset.ItemModel;
            else FemaleWorldModel.Model = asset.ItemModel;
        }
        public void CopyAlternateTextureSets(ModelAlternateTextures male, ModelAlternateTextures female) {
            MaleWorldModel.CopyAlternateTexturesFrom(male);
            FemaleWorldModel.CopyAlternateTexturesFrom(female);
        }
        public void SaveDefaultSetting() => DefaultEquipmentsValue.Add(new DefaultEquipmentsValue(this,EditorID));
    }
    public class Armatures : RecordArrays<Handle>
    {
        public static string Signature => "Armature";
        public override string signature => Signature;
        Armor armor;
        public string Name;
        public Armatures(Armor armor):base(armor.handle)
        {
            this.armor = armor;
            AddRange(handle.GetArrayItems("MODL").ToList());
        }
        public bool[] Add(params ArmorAddon[] items)
        {
            var results = new bool[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (Count == 0)
                {
                    armor.handle.SetValue(@"Armature\MODL", item.handle.GetFormID() );
                    Add(handle.GetElement("MODL"));
                }
                else
                {
                    if (!Exists(item))
                    {
                        Add(Elements.AddArrayItem(handle, "", "", item.handle.GetFormID()));
                        results[i] = true;
                    }
                }
            }
            return results;
        }
        public void Remove(ArmorAddon item)
        {
            var exist = Find(item);
            if (exist != null)
            {
                exist.Delete();
                Remove(exist);
            }
        }
        public bool Exists(ArmorAddon armorAddon) => Exists(d => d.GetValue() == armorAddon.ToString());
        public Handle Find(ArmorAddon armorAddon) => Find(d => d.GetValue() == armorAddon.ToString());
        public ArmorAddon GetArmorAddon(int index) {
            foreach (var armorAddon in armor.plugin.ArmorAddons)
                if (armorAddon.ToString() == this[index].GetValue()) return armorAddon;
            return null;
        }
        public new void RemoveAt(int index)
        {
            if (index < Count) {
                this[index].Delete();
                RemoveAt(index);
            }
        }
        public new void Clear() {
            foreach (var item in this) item.Delete();
            handle.Delete(); // Armature need delete while clear
            base.Clear();
        }
        public override string ToString() => string.Join("\n", this);

    }
}
