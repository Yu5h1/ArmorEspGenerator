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

        public static string RaceKey => "RNAM - Race";
        public string Race {
            get => handle.GetElement(RaceKey).GetValue();
            set => handle.SetValue(RaceKey, value);
        }

        public BipedBodyTemplate bipedBodyTemplate;
        public ModelAlternateTextures MaleWorldModel;
        public ModelAlternateTextures FemaleWorldModel;
        public ModelAlternateTextures Male1stPerson;
        public ModelAlternateTextures Female1stPerson;
        ArmorAddon(PluginRecords<ArmorAddon> container, Handle target) : base(container, target) {
            bipedBodyTemplate = new BipedBodyTemplate(target);
            MaleWorldModel = new ModelAlternateTextures(target, true);
            FemaleWorldModel = new ModelAlternateTextures(target, false);
            Male1stPerson = new ModelAlternateTextures(target, true,true);
            Female1stPerson = new ModelAlternateTextures(target, false, true);
        }
        public void SetModelAssets(bool maleOrFemale,EquipmentAsset asset) {
            if (asset != null)
            {
                var worldModel = maleOrFemale ? MaleWorldModel : FemaleWorldModel;
                var _1st = maleOrFemale ? Male1stPerson : Female1stPerson;
                if (asset.Model != "") {
                    worldModel.Model = asset.Model;
                    bipedBodyTemplate.SetPartitionsFromNif(asset.Model);
                }
                if (asset._1stPerson != "") _1st.Model = asset._1stPerson;
            }
        }

        public static ArmorAddon Create(PluginRecords<ArmorAddon> container, Handle handle)
        {
            if (handle.CompareSignature<ArmorAddon>()) return new ArmorAddon(container, handle);
            else return null;
        }

        public ModelAlternateTextures GetWorldModel(bool MaleOrFemale) => MaleOrFemale ? MaleWorldModel : FemaleWorldModel;
        public override string GetDataInfo() => ToString()+'\n'+ FemaleWorldModel.ToString();
        public static string MakeArmorAddonName(string txt, string suffix = "") => txt.MakeValidEditorID().RemoveSuffixFromLast("AA").TrimEndNumber() + suffix + "AA";
    }
}
