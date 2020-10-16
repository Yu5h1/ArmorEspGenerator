using System;
using System.Collections.Generic;
using System.IO;
using Yu5h1Tools.WPFExtension;

using System.Text.RegularExpressions;
using System.Linq;

namespace TESV_EspEquipmentGenerator
{
    public partial class Plugin
    {
        static string TrimSourceFolderPath(string value, string folder) {
            if (value.Equals(string.Empty)) return value;
            int index = value.ToLower().IndexOf(folder.ToLower());
            if (index >= 0) value = value.Substring(index + folder.Length).RemovePrefixUtilEmpty(@"\");
            return value;
        }
        public static string TrimTexturesPath(string value) => TrimSourceFolderPath(value, GetTexturesPath());
        public static string TrimMeshesPath(string value) => TrimSourceFolderPath(value, GetMeshesPath());

        public void AddTextureSetsByNif(string filename)
        {
            var shapesTextures = NifUtil.GetShapesTextureInfos(filename);
            foreach (var item in shapesTextures)
            {
                var textureSet = TextureSets.AddNewItem(item.Key);
                textureSet.CopyTexturePath(item.Value);
            }
        }

        public List<TextureSet> AddTextureSetsByDifuseAssets(string[] defaultTexturesPath, params string[] files)
        {
            var results = new List<TextureSet>();
            foreach (var path in files)
            {
                if (!IsLocateAtGameAssetsFolder(path)) return null;

                var DifusePath = TrimTexturesPath(path);
                var textureSet = TextureSets.Find(d => d.Difuse.Equals(DifusePath, StringComparison.OrdinalIgnoreCase));
                if (textureSet == null)
                {
                    textureSet = TextureSets.AddNewItem(Path.GetFileNameWithoutExtension(path));
                    textureSet.Difuse = DifusePath;
                }
                if (defaultTexturesPath != null)
                    if (defaultTexturesPath.Length > 7)
                        textureSet.CopyTexturePath(defaultTexturesPath, 0);

                if (textureSet) results.Add(textureSet);
                else "Create New TextureSet Failed ! ".PromptWarnning();
            }
            return results;
        }
        public void GenerateEspByFolder(string shapeName)
        {

        }
        public void GenerateArmorsByFolder(string FolderPath)
        {
            PathInfo folderPathInfo = new PathInfo(FolderPath);
            var nifFiles = Directory.GetFiles(FolderPath, "*_1.nif");
            foreach (var nif in nifFiles)
            {
                string name = PathInfo.GetName(nif).RemoveSuffixFrom("_1");
                ArmorAddon newArmorAddon = AddArmorAddonByNifFile(nif);

                Armor armor = AddArmorByArmaturesFromArmorAddon(name, newArmorAddon);
                armor.FULLName = name;
            }
        }

        public ArmorAddon AddArmorAddonByNifFile(string nifPath, bool CheckExists = true)
        {
            var nifPathInfo = new PathInfo(nifPath);
            if (InformationViewer.IsFileExistElsePrompt(nifPath))
            {
                if (CheckExists)
                {
                    foreach (var item in ArmorAddons)
                    {
                        ArmorAddon usedArmorAddon = null;
                        if (GetMeshesPath(item.MaleWorldModel.Model).Equals(nifPath, StringComparison.OrdinalIgnoreCase) ||
                            GetMeshesPath(item.FemaleWorldModel.Model).Equals(nifPath, StringComparison.OrdinalIgnoreCase))
                            usedArmorAddon = item;
                        if (usedArmorAddon)
                        {
                            ("Model has already been used by " + usedArmorAddon.ToString()).PromptWarnning();
                            return null;
                        }
                    }
                }

                var editorID = nifPathInfo.Name.RemoveSuffixFrom("_1");
                bool maleOrfemale = true;
                if (nifPathInfo.directoryName.MatchAny("f", "female"))
                {
                    maleOrfemale = false;
                }
                else if (nifPathInfo.directoryName.MatchAny("m", "male"))
                {
                }
                else if (editorID.EndsWith("F"))
                {
                    editorID = editorID.Remove(editorID.Length - 1);

                    maleOrfemale = false;
                }
                else
                {
                    if (editorID.EndsWith("M")) editorID = editorID.Remove(editorID.Length - 1);


                }

                string ArmorAddonName = ArmorAddon.MakeArmorAddonName(editorID);
                ArmorAddon armorAddon = null;
                foreach (var item in ArmorAddons)
                {
                    if (item.EditorID == ArmorAddonName)
                    {
                        armorAddon = item;
                        break;
                    }
                }
                if (armorAddon == null) armorAddon = ArmorAddons.AddNewItem(ArmorAddonName);

                if (maleOrfemale)
                {
                    armorAddon.MaleWorldModel.Model = nifPath;
                }
                else
                {
                    armorAddon.FemaleWorldModel.Model = nifPath;
                }

                if (File.Exists(nifPathInfo.ChangeName(editorID + "_gnd")))
                {

                }
                return armorAddon;
            }
            return null;
        }

        public Armor AddArmorByArmaturesFromArmorAddon(string editorID, params ArmorAddon[] armorAddons)
                                    => AddArmorByArmaturesFromArmorAddons(editorID,armorAddons);
        public Armor AddArmorByArmaturesFromArmorAddons(string editorID,IEnumerable<ArmorAddon> armorAddons) {
            
            foreach (var AO in Armors)
            {
                if (AO.armatures.Count == armorAddons.Count()) {
                    bool allMatch = true;
                    foreach (var AA in armorAddons)
                    {
                        if (!AO.armatures.Exists(AA))
                        {
                            allMatch = false;
                            break;
                        }
                    }
                    if (allMatch)
                    {
                        ("Armatures already exists.Which is " + AO.ToString()).PromptWarnning();
                        return AO;
                    }
                }
            }
            var newArmor = Armors.AddNewItem(editorID);
            var flags = BipedBodyTemplate.GetPartitionFlags();
            foreach (var item in armorAddons) {
                newArmor.armatures.Add(item);
                for (int i = 0; i < flags.Length; i++)
                {
                    if (item.bipedBodyTemplate.FirstPersonFlags[i].IsEnable)
                        flags[i].IsEnable = true;
                }
            }
            newArmor.bipedBodyTemplate.FirstPersonFlags = flags;
            return newArmor;
        }
        public void GenerateArmorsBySimilarDiffuses(ArmorAddon defaultArmorAddon)
        {
            bool MaleOfFemale = false;
            var worldModel = defaultArmorAddon.GetWorldModel(MaleOfFemale);

            for (int shapeIndex = 0; shapeIndex < worldModel.ShapesNames.Count; shapeIndex++)
            {
                var shapeName = worldModel.ShapesNames[shapeIndex];
                if (shapeName.MatchAny("UUNP", "Body", "Hands", "Foot", "BaseShape")) continue;

                
                //TextureSet.FindSimilarDiffuseTextures();
                //var textureSets = worldModel.AddTextsetsBySimilarDiffuses(targetShapeIndex);
                //foreach (var textureset in textureSets)
                //{
                //    var difuseSuffix = textureset.Difuse.NameWithOutExtension().RemovePrefixTo("_").MakeValidEditorID().FirstCharToUpper();
                //    var newArmorAddon = plugin.ArmorAddons.Duplicate(targetAA, ArmorAddon.MakeArmorAddonName(targetAA.EditorID, difuseSuffix));
                //    var newWorldModel = newArmorAddon.GetWorldModel(MaleOfFemale);
                //    newWorldModel.alternateTextures.Clear();
                //    newWorldModel.alternateTextures.Set(worldModel.ShapesNames[targetShapeIndex], textureset);
                //    var newArmor = plugin.Armors.Duplicate(this, EditorID.TrimEndNumber() + difuseSuffix);
                //    newArmor.FULLName = FULLName.RemoveSuffixFrom("_", " ").TrimEndNumber() + " " + difuseSuffix;
                //    newArmor.armatures.Clear();
                //    newArmor.armatures.Add(newArmorAddon);
                //}
            }

        }


        public static List<int> GetBodyPartsIndicesFromNif(string path) {
            List<int> bodyPartsIndices = new List<int>();
            var shapesPartitions = NifUtil.GetBSDismemberBodyParts(path).GetLines();
            foreach (var item in shapesPartitions)
            {
                var infos = item.Split(':');
                infos[infos.Length-1].Split(',').ToList().ForEach(obj =>
                {
                    if (int.TryParse(obj, out int bsdBodyPartIndex))
                        if (!bodyPartsIndices.Contains(bsdBodyPartIndex)) bodyPartsIndices.Add(bsdBodyPartIndex);
                });
            }
            bodyPartsIndices.Sort();
            return bodyPartsIndices;
        }
    }
}
