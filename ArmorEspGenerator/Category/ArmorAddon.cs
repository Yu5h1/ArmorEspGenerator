using System;
using System.Collections.Generic;
//using System.Linq;
using System.IO;
using System.Threading.Tasks;
using XeLib.API;
using XeLib;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public class ArmorAddon : RecordElement<ArmorAddon>
    {                
        public const string Signature = "ARMA";
        public override string signature => Signature;

        public BipedBodyTemplate bipedBodyTemplate;
        public WorldModel MaleWorldModel;
        public WorldModel FemaleWorldModel;

        ArmorAddon(PluginRecords<ArmorAddon> container, Handle target) : base(container, target) {
            bipedBodyTemplate = new BipedBodyTemplate(handle,handle.GetElement(BipedBodyTemplate.Signature));
            MaleWorldModel = new WorldModel(this, true);
            FemaleWorldModel = new WorldModel(this, false);
        }
        public static ArmorAddon Create(PluginRecords<ArmorAddon> container, Handle handle)
        {
            if (handle.CompareSignature<ArmorAddon>()) return new ArmorAddon(container, handle);
            else return null;
        }

        public WorldModel GetWorldModel(bool MaleOrFemale) => MaleOrFemale ? MaleWorldModel : FemaleWorldModel;
        public override string GetDataInfo() => ToString()+'\n'+ FemaleWorldModel.ToString();
        public static string MakeArmorAddonName(string txt, string suffix = "") => txt.MakeValidEditorID().RemoveSuffixFrom("AA").TrimEndNumber() + suffix + "AA";
    }
}
