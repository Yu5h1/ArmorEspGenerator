using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeLib.API;
using XeLib;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public class Armor : RecordElement<Armor>
    {
        public const string Signature = "ARMO";
        public override string signature => Signature;
        public string FULLName { get => GetValue(FullNameKEY); set => SetValue(FullNameKEY, value); }
        public BipedBodyTemplate bipedBodyTemplate;
        public ModelAlternateTextures MaleWorldModel;
        public ModelAlternateTextures FemaleWorldModel;
        public Armatures armatures;
        public static string RaceKey => "RNAM - Race";
        public string Race
        {
            get => handle.GetElement(RaceKey).GetValue();
            set => handle.SetValue(RaceKey, value);
        }

        Armor(PluginRecords<Armor> container, Handle target) : base(container, target) {
            armatures = new Armatures(this);
            bipedBodyTemplate = new BipedBodyTemplate(target);
            MaleWorldModel = new ModelAlternateTextures(target, true);
            FemaleWorldModel = new ModelAlternateTextures(target, false);
        }
        public static Armor Create(PluginRecords<Armor> container, Handle handle) {
            if (handle.CompareSignature<Armor>()) return new Armor(container, handle);
            else return null;
        }
        public void SetModelAssets(bool maleOrfemale,EquipmentAsset asset) {
            if (asset == null) return;
            if (asset.ItemModel == "") return;
            if (maleOrfemale) MaleWorldModel.Model = asset.ItemModel;
            else FemaleWorldModel.Model = asset.ItemModel;
        }
    }
    public class Armatures : RecordArrayObject<Handle>
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
