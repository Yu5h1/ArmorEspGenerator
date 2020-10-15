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
        #region recordsPath
        const string MWMm2 = @"Male world Model\MOD2";
        const string FWMm4 = @"Female world Model\MOD4";
        #endregion
        public string FULLName { get => GetValue("FULL"); set => SetValue("FULL", value); }
        //public string MaleModel { get => GetValue(MWMm2); set => SetValue(MWMm2, value); }
        //public string FemaleModel { get => GetValue(FWMm4); set => SetValue(FWMm4, value); }
        public BipedBodyTemplate bipedBodyTemplate;
        public WorldModel MaleWorldModel;
        public WorldModel FemaleWorldModel;
        public Armatures armatures;


        Armor(PluginRecords<Armor> container, Handle target) : base(container, target) {
            armatures = new Armatures(this);
            bipedBodyTemplate = new BipedBodyTemplate(handle, handle.GetElement(BipedBodyTemplate.Signature));
            MaleWorldModel = new WorldModel(this, true);
            FemaleWorldModel = new WorldModel(this, false);
        }
        public static Armor Create(PluginRecords<Armor> container, Handle handle) {
            if (handle.CompareSignature<Armor>()) return new Armor(container, handle);
            else return null;
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
                var label = item.ToString();
                if (Count == 0)
                {
                    armor.handle.SetValue(@"Armature\MODL", label);
                    Add(handle.GetElement("MODL"));
                }
                else
                {
                    if (!Exists(item))
                    {
                        Add(Elements.AddArrayItem(handle, "", "", label));
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
