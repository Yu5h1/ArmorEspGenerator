using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XeLib.API;
using XeLib;
using Yu5h1Tools.WPFExtension;
using System.Linq;

namespace TESV_EspEquipmentGenerator
{
    public class ArmorAddon : RecordElement<ArmorAddon>
    {
        public class DNAMData : RecordObject
        {
            public static string Signature => "DNAM - Data";
            public override string signature => Signature;

            public int malePriority {
                get => handle.GetInteger("Male Priority");
                set => handle.SetInteger("Male Priority",value);
            }
            public int femalePriority
            {
                get => handle.GetInteger("Female Priority");
                set => handle.SetInteger("Female Priority", value);
            }
            public Handle WeightSliderMale => handle.GetElement("Weight slider - Male");
            public bool EnableWeightSliderMale {
                get => ElementValues.GetFlag(WeightSliderMale, "", "Enabled");
                set => ElementValues.SetFlag(WeightSliderMale, "", "Enabled",value);
            }
            public Handle WeightSliderFemale => handle.GetElement("Weight slider - Female");
            public bool EnableWeightSliderFemale
            {
                get => ElementValues.GetFlag(WeightSliderFemale, "", "Enabled");
                set => ElementValues.SetFlag(WeightSliderFemale, "", "Enabled", value);
            }
            public DNAMData(Handle Parent) : base(Parent) {}

        }
        public const string Signature = "ARMA";
        public override string signature => Signature;
        
        public string Race {
            get => handle.GetElement(RaceKey).GetValue();
            set => handle.SetValue(RaceKey, value);
        }
        public const string AdditionalRacesKey = "Additional Races";
        public Handle AdditionalRaces
        {
            get => handle.GetElement("Additional Races");
        }
        public void AddDefaultRaces() {
            foreach (var item in new string[] {"00013740","00013741","00013742","00013743","00013744","00013745",
                    "00013746","00013747","00013748","00013749","00067CD8","00088794","0008883A",
                    "0008883C","0008883D","00088840","00088844","00088845","00088846","00088884",
                    "000A82B9","000A82BA","0010760A" })
            {
                handle.AddArrayItem("Additional Races", "", item);
            }
        }
        public DNAMData DNAM_Data;
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
            DNAM_Data = new DNAMData(handle);
        }
        public void SetModelAssets(EquipmentAsset asset,ModelAlternateTextures worldModel, ModelAlternateTextures _1st) {
            if (asset != null)
            {
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
        public static string MakeArmorAddonName(string txt, string suffix = "") => txt.MakeValidEditorID().TrimAfterLast("AA").TrimEndNumber() + suffix + "AA";

        public List<ArmorAddon> DuplicateByShapeDiffuse(out string[] tags,params string[] ignoreDiffuseNames) {
            int femaleShapeIndex = 0, maleShapeIndex = 0;
            tags = new string[0];
            var results = new List<ArmorAddon>();
            try
            {
                var femaleModelPath = Plugin.GetMeshesPath(FemaleWorldModel.Model);
                var maleModelPath = Plugin.GetMeshesPath(MaleWorldModel.Model);

                var femaleTexturesInfo = NifUtil.GetShapesTextureInfos(femaleModelPath);
                var maleTexturesInfo = NifUtil.GetShapesTextureInfos(maleModelPath);

                var defaultFemaleTextureSet = new string[] { "" };
                if (femaleShapeIndex < femaleTexturesInfo.Count)
                    defaultFemaleTextureSet = femaleTexturesInfo[femaleShapeIndex].textures;
                var defaultMaleTextureSet = new string[] { "" };
                if (maleShapeIndex < maleTexturesInfo.Count)
                    defaultMaleTextureSet = maleTexturesInfo[maleShapeIndex].textures;

                if (ignoreDiffuseNames.Length > 0)
                {
                    while (femaleShapeIndex < femaleTexturesInfo.Count &&
                            PathInfo.GetName(defaultFemaleTextureSet[0]).MatchAny(ignoreDiffuseNames))
                    {
                        femaleShapeIndex++;
                        defaultFemaleTextureSet = femaleTexturesInfo[femaleShapeIndex].textures;
                    }
                    while (maleShapeIndex < maleTexturesInfo.Count &&
                            PathInfo.GetName(defaultMaleTextureSet[0]).MatchAny(ignoreDiffuseNames))
                    {
                        maleShapeIndex++;
                        defaultMaleTextureSet = maleTexturesInfo[maleShapeIndex].textures;
                    }
                }

                var ShareTXSTshapeIndicesF = NifUtil.GetShareTexturesShapesIndices(femaleModelPath, femaleShapeIndex);
                var ShareTXSTshapeIndicesM = NifUtil.GetShareTexturesShapesIndices(maleModelPath, maleShapeIndex);

                var femaleTextures = TextureSet.FindSimilarDiffuseTextures(Plugin.GetTexturesPath(defaultFemaleTextureSet[0]), out string[] femaleDiffuseTags);
                var maleTextures = TextureSet.FindSimilarDiffuseTextures(Plugin.GetTexturesPath(defaultMaleTextureSet[0]), out string[] maleDiffuseTags);

                
                
                var femaleTextureSets = plugin.AddTextureSetsByDifuseAssets(false, defaultFemaleTextureSet, femaleTextures);
                var maleTextureSets = plugin.AddTextureSetsByDifuseAssets(false, defaultMaleTextureSet, maleTextures);

                tags = femaleDiffuseTags.Length > maleDiffuseTags.Length ? femaleDiffuseTags : maleDiffuseTags;

                try
                {
                    for (int i = 0; i < tags.Length; i++)
                    {
                        var newAA = Duplicate(EditorID.Insert(EditorID.LastIndexOf("AA"), tags[i])) as ArmorAddon;
                        if (i < femaleTextureSets.Count)
                        {
                            foreach (var index in ShareTXSTshapeIndicesF)
                            {
                                newAA.FemaleWorldModel.alternateTextures.
                                        Set(newAA.FemaleWorldModel.ShapesNames[index], femaleTextureSets[i]);
                            }
                        }
                        if (i < maleTextureSets.Count)
                        {
                            foreach (var index in ShareTXSTshapeIndicesM)
                            {
                                newAA.MaleWorldModel.alternateTextures.
                                    Set(newAA.MaleWorldModel.ShapesNames[index], maleTextureSets[i]);
                            }
                        }
                        results.Add(newAA);
                    }

                } catch (Exception error)
                {
                    ("DuplicateByShapeDiffuse assign new armoraddon" + error.Message).PopupError();
                    throw;
                }

            } catch (Exception error)
            {
                ("DuplicateByShapeDiffuse " + error.Message).PopupError();
                throw;
            }
            return results;
        }
    }
    
}
