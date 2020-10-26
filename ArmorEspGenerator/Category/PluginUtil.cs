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
            if (string.IsNullOrEmpty(value)) return value;
            int index = value.ToLower().IndexOf(folder.ToLower());
            if (index >= 0) value = value.Substring(index + folder.Length).RemovePrefixUtilEmpty(@"\");
            return value;
        }
        public static string TrimTexturesPath(string value) => value.TrimPrefix("textures",1);
        public static string TrimMeshesPath(string value) => value.TrimPrefix("meshes", 1);

        public void AddTextureSetsByNif(string filename)
        {
            try
            {
                var shapesTextures = NifUtil.GetShapesTextureInfos(filename);
                foreach (var item in shapesTextures)
                {
                    item.Value.Switch(2,5);
                    item.Value.Switch(4,5);
                    item.Value.Switch(3,4);
                }
                foreach (var item in shapesTextures)
                {
                    var textureSet = TextureSets.AddNewItem(item.Key);
                    textureSet.CopyTexturePath(item.Value);
                }
            }
            catch (Exception error)
            {
                error.Message.PromptError();
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
        //public static string ItemModelTag = "go";//"go" "gnd";
        public bool CheckIsItemModel(ref string key,params string[] tags) {
            foreach (var tag in tags)
            {
                if (key.EndsWith(tag))
                {
                    key = key.Remove(key.Length - tag.Length);
                    return true;
                }
            }
            return false;
        }
        public static StringComparison MaleFemaleComparison = StringComparison.OrdinalIgnoreCase;
        public static bool ShareGNDIfEmpty = true;
        public void GetSortedNifsFromFolder(Dictionary<string, EquipmentAssets> datas,string folderPath,bool? MaleOrFemale = null) {
            if (!Directory.Exists(folderPath)) return;
            PathInfo folderPathInfo = new PathInfo(folderPath);
            foreach (var path in Directory.GetFiles(folderPath, "*.nif").Where(d => !d.Contains("_0")))
            {
                bool IsItemModel = false, Is1stperson = false,IsMaleOrFemale = true;
                var pathinfo = new PathInfo(path);
                var nameParts = pathinfo.Name.Split('_').ToList();
                var key = nameParts[0].ToLower();
                if (key.Contains("1stperson"))
                {
                    key = key.TrimPrefix("1stperson");
                    Is1stperson = true;
                }
                IsItemModel = CheckIsItemModel(ref key, "gnd", "go");

                if (MaleOrFemale == null)
                {
                    IsMaleOrFemale = !key.EndsWith("F", MaleFemaleComparison) ||  
                                                        nameParts.ExistsAny( (ele,arg) => 
                                                        ele.Equals(arg,StringComparison.OrdinalIgnoreCase),
                                                        "f","female");
                    if (key.EndsWith("F", MaleFemaleComparison) || key.EndsWith("M", MaleFemaleComparison))
                        key = key.Remove(key.Length - 1);
                }
                else IsMaleOrFemale = (bool)MaleOrFemale;
                if (nameParts.Count > 2)
                {
                    for (int i = 1; i < nameParts.Count - 1; i++) {
                        if (!nameParts[i].MatchAny("m", "male", "f", "female")) {
                            key += "_" + nameParts[i];
                        }
                    }
                }
                if (!datas.ContainsKey(key)) datas.Add(key, new EquipmentAssets());
                EquipmentAsset data = IsMaleOrFemale ? datas[key].male : datas[key].female;
                if (IsItemModel) data.ItemModel = path;
                else if (Is1stperson) data._1stPerson = path;
                else data.Model = path;
            }
        }
        
        public void GenerateArmorsByFolder(string folderPath)
        {
            PathInfo folderPathInfo = new PathInfo(folderPath);
            var datas = new Dictionary<string, EquipmentAssets>();
            string  MaleFolder = folderPathInfo.FindAny("m", "male"),
                    FemaleFolder = folderPathInfo.FindAny("f", "female");

            if (folderPathInfo.FindAny("f", "female", "m", "male") == "")
            {
                GetSortedNifsFromFolder(datas, folderPath);
            }
            else
            {
                GetSortedNifsFromFolder(datas, MaleFolder, true);
                GetSortedNifsFromFolder(datas, FemaleFolder, false);
            }
            Dictionary<string, Armor> newArmors = new Dictionary<string, Armor>();
            foreach (var item in datas)
            {
                var keys = item.Key.Split('_');
                string key = keys[0];
                //(item.Key + "::" +key + "\n" + item.Value.male.ToString() + "\n" + item.Value.female.ToString()).PromptInfo();
                var editorID = folderPathInfo.Name.Capitalize() + item.Key.Capitalize();
                ArmorAddon newArmorAddon = ArmorAddons.AddNewItem(editorID + "AA");
                newArmorAddon.SetModelAssets(true, item.Value.male);
                newArmorAddon.SetModelAssets(false, item.Value.female);
                newArmorAddon.Race = "DefaultRace \"Default Race\" [RACE:00000019]";
                
                if (!newArmors.TryGetValue(key, out Armor newArmor))
                {
                    newArmor = newArmor = Armors.AddNewItem(editorID + "AO");
                    newArmor.bipedBodyTemplate.FirstPersonFlags = newArmorAddon.bipedBodyTemplate.FirstPersonFlags;
                    newArmor.bipedBodyTemplate.ArmorType = newArmorAddon.bipedBodyTemplate.ArmorType;
                    newArmor.FULLName = folderPathInfo.Name.Capitalize() + " " + item.Key.Capitalize();
                    newArmors.Add(key, newArmor);
                }
                newArmor.SetModelAssets(true, item.Value.male);
                newArmor.SetModelAssets(false, item.Value.female);
                newArmor.armatures.Add(newArmorAddon);
            }
            if (ShareGNDIfEmpty) {
                foreach (var newArmor in newArmors)
                {
                    if (newArmor.Value.MaleWorldModel.Model == "" && newArmor.Value.FemaleWorldModel.Model == "")
                        continue;
                    if (newArmor.Value.FemaleWorldModel.Model == "" && newArmor.Value.MaleWorldModel.Model != "")
                        newArmor.Value.FemaleWorldModel.Model = newArmor.Value.MaleWorldModel.Model;
                    if (newArmor.Value.MaleWorldModel.Model == "" && newArmor.Value.FemaleWorldModel.Model != "")
                        newArmor.Value.MaleWorldModel.Model = newArmor.Value.FemaleWorldModel.Model;
                }
            }
            return;
            //var nifFiles = Directory.GetFiles(FolderPath, "*_1.nif", SearchOption.AllDirectories).
            //                                                    Where(d => !d.Contains("1stperson"));
            //List<ArmorAddon> newArmorAddons = new List<ArmorAddon>();
            //List<Armor> newArmorArmors = new List<Armor>();
            //foreach (var nif in nifFiles)
            //{
            //    var nifPathInfo = new PathInfo(nif);
            //    string name = PathInfo.GetName(nif).RemoveSuffixFrom("_1");
            //    ArmorAddon newArmorAddon = AddArmorAddonByNifFile(nif, folderPathInfo.Name);
            //    if (!newArmorAddons.Contains(newArmorAddon))
            //    {
            //        Armor newArmor = AddArmorByArmaturesFromArmorAddon(newArmorAddon);
            //        newArmorArmors.Add(newArmor);
            //        newArmorAddons.Add(newArmorAddon);
            //    }
            //}
        }

        public ArmorAddon AddArmorAddonByNifFile(string nifPath,string Nameprfix = "", bool CheckExists = true)
        {
            var nifPathInfo = new PathInfo(nifPath);
            var nifDir = new PathInfo(nifPathInfo.directory);
            var model1stpersonPath = nifDir.CombineWith("1stperson" + nifPathInfo.FileName);
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
                var baseName = nifPathInfo.Name.Remove(nifPathInfo.Name.IndexOf("_"));
                var editorID = baseName;
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
                string ArmorAddonName = Nameprfix.MakeValidEditorID()+ ArmorAddon.MakeArmorAddonName(editorID);

                ArmorAddon armorAddon = ArmorAddons.Find(d => d.EditorID == ArmorAddonName);

                if (armorAddon == null) armorAddon = ArmorAddons.AddNewItem(ArmorAddonName);

                if (maleOrfemale)
                {
                    armorAddon.MaleWorldModel.Model = nifPath;
                    if (File.Exists(model1stpersonPath))
                        armorAddon.Male1stPerson.Model = model1stpersonPath;
                }
                else
                {
                    armorAddon.FemaleWorldModel.Model = nifPath;
                    if (File.Exists(model1stpersonPath))
                        armorAddon.Female1stPerson.Model = model1stpersonPath;
                }
                return armorAddon;
            }
            return null;
        }

        public Armor AddArmorByArmaturesFromArmorAddon(params ArmorAddon[] armorAddons)
                                    => AddArmorByArmaturesFromArmorAddons(armorAddons);
        public Armor AddArmorByArmaturesFromArmorAddons(IEnumerable<ArmorAddon> armorAddons) {

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
            var firstArmorAddon = armorAddons.First();
            var newArmorID = firstArmorAddon.EditorID.RemoveSuffixFromLast("AA")+"AO";
            var newArmor = Armors.AddNewItem(newArmorID);
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
            if (armorAddons.Count() == 1)
            {                
                var modelPathInfo = new PathInfo(GetMeshesPath(firstArmorAddon.MaleWorldModel.Model));
                var gndModelPath = modelPathInfo.ChangeName(modelPathInfo.Name.RemoveSuffixFrom("_") + "gnd");
                gndModelPath.PromptInfo();
            }
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
