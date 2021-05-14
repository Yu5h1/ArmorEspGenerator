using System;
using System.Collections.Generic;
using System.IO;
using Yu5h1Tools.WPFExtension;
using System.Linq;
using XeLib.API;
using System.Diagnostics;

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
        public static string TrimTexturesPath(string value) => value.TrimBeforeLast("textures",true).TrimStartSlash();
        public static string TrimMeshesPath(string value) => value.TrimBeforeLast("meshes", true).TrimStartSlash();

        public void AddTextureSetsByNif(string filename)
        {
            try
            {
                foreach (var item in NifUtil.GetShapesTextureInfos(filename).TransferToTextureSetOrder())
                {
                    var textureSet = TextureSets.AddNewItem(item.shapeName);
                    textureSet.CopyTexturePath(item.textures);
                }
            }
            catch (Exception error)
            {
                error.Message.PopupError();
            }
        }

        public List<TextureSet> AddTextureSetsByDifuseAssets(bool PopupLog,string[] defaultTexturesPath, params string[] files)
        {
            var results = new List<TextureSet>();
            foreach (var path in files)
            {
                try
                {
                    var pathinfo = new PathInfo(path);
                    if (!IsLocateAtGameAssetsFolder(path)) return null;
                    var DifusePath = TrimTexturesPath(path);
                    var textureSet = TextureSets.Find(d => d.Difuse.Equals(DifusePath, StringComparison.OrdinalIgnoreCase));
                    if (textureSet == null)
                    {
                        textureSet = TextureSets.AddNewItem(PathInfo.Replace(pathinfo.Name, "_D", ""));
                        textureSet.Difuse = DifusePath;                        
                    } else if (PopupLog) (textureSet.Difuse + " already exists.Which FormId is " + textureSet.FormID).PopupInfo();

                    if (defaultTexturesPath != null && defaultTexturesPath.Length > 7) {
                        textureSet.CopyTexturePath(defaultTexturesPath, 0);
                    }
                    if (textureSet)
                    {
                        textureSet.FindTexturesByDiffuse();
                        results.Add(textureSet);
                    } else "Create New TextureSet Failed ! ".PopupWarnning();
                } catch (Exception error)
                {
                    error.Message.PopupError();
                    throw;
                }

            }
            return results;
        }
        public static StringComparison MaleFemaleStringComparison = StringComparison.Ordinal;
        public static bool ShareGNDIfEmpty = true;
        public void GetSortedNifsFromFolder(Dictionary<string, EquipmentAssets> datas, string folderPath, bool? MaleOrFemale = null) {
            if (!Directory.Exists(folderPath)) return;
            PathInfo folderPathInfo = new PathInfo(folderPath);
            foreach (var path in Directory.GetFiles(folderPath, "*.nif").Where(d => !d.Contains("_0")))
            {
                bool Is1stperson = false, IsMaleOrFemale = true;
                var pathinfo = new PathInfo(path);

                bool IsItemModel = NifUtil.IsGroundItemObject(pathinfo) ||
                                   pathinfo.Name.Contains("Gnd", "Go", "GND", "GO", "_gnd", "_Gnd", "_GND");

                var nameParts = pathinfo.Name.Split('_').ToList();
                var key = nameParts[0];
                if (key.Contains("1stperson",StringComparison.OrdinalIgnoreCase))
                {
                    key = key.TrimBeforeLast("1stperson");
                    Is1stperson = true;
                }
                if (MaleOrFemale == null)
                {                    
                    IsMaleOrFemale = !key.EndsWith("F", MaleFemaleStringComparison) ||
                                                        !nameParts.ExistsAny( (ele, arg) =>
                                                        ele.Equals(arg, StringComparison.OrdinalIgnoreCase),
                                                        "f", "female");
                    if (key.EndsWith("F", MaleFemaleStringComparison) || key.EndsWith("M", MaleFemaleStringComparison))
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
        public bool GenerateArmorsBySpeculateFolder(string folderPath, Func<double,bool> progress ) {
            bool canceled = false;
            var targetFolders = Directory.GetDirectories(folderPath, "*").ToList();
            targetFolders.Add(folderPath);
            for (int i = 0; i < targetFolders.Count; i++)
            {
                if (!PathInfo.GetName(targetFolders[i]).MatchAny("f", "m", "female", "male")) 
                    GenerateArmorsByFolder(targetFolders[i], (p) => {
                        canceled = progress((i + p) / targetFolders.Count);
                        return canceled;
                    });
            }
            return canceled;
        }
        public void GenerateArmorsByFolder(string folderPath, Func<double,bool> progress)
        {
            DefaultEquipmentsValue.Load();
            PathInfo folderPathInfo = new PathInfo(folderPath);
            string NameSet = folderPathInfo.Name.Capitalize();
            var datas = new Dictionary<string, EquipmentAssets>(StringComparer.OrdinalIgnoreCase);
            string MaleFolder = folderPathInfo.FindAny("m", "male"),
                   FemaleFolder = folderPathInfo.FindAny("f", "female");
            try
            {
                if (folderPathInfo.FindAny("f", "female", "m", "male") == "")
                {
                    GetSortedNifsFromFolder(datas, folderPath);
                }
                else
                {
                    GetSortedNifsFromFolder(datas, MaleFolder, true);
                    GetSortedNifsFromFolder(datas, FemaleFolder, false);
                }
            }
            catch (Exception error)
            {
                ("Preparing Keys encountering Exception ->\n" + error.Message).PopupError();
                //throw;
            }
            Dictionary<string, Armor> newArmors = new Dictionary<string, Armor>(StringComparer.OrdinalIgnoreCase);
            List<ArmorAddon> newArmorAddons = new List<ArmorAddon>();
            Dictionary<string, Armor> AlternateTexturesNewArmors = new Dictionary<string, Armor>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<ArmorAddon>> AlternateTexturesArmorAddonSets = new Dictionary<string, List<ArmorAddon>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                double phaseValue = 0;
                foreach (var item in datas)
                {
                    phaseValue += 1;
                    if (progress(phaseValue / datas.Count)) return;
                    var keys = item.Key.Split('_').ToList();
                    string key = keys[0];

                    //(item.Key + "::" +key + "\n" + item.Value.male.ToString() + "\n" + item.Value.female.ToString()).PopupInfo();
                    var editorID = NameSet + item.Key.Capitalize();
                    ArmorAddon newArmorAddon = ArmorAddons.AddNewItem(editorID + "AA");
                    newArmorAddons.Add(newArmorAddon);
                    newArmorAddon.SetModelAssets(item.Value.male, newArmorAddon.MaleWorldModel, newArmorAddon.Male1stPerson);
                    newArmorAddon.SetModelAssets(item.Value.female, newArmorAddon.FemaleWorldModel, newArmorAddon.Female1stPerson);
                    newArmorAddon.DNAM_Data.malePriority = 5;
                    newArmorAddon.DNAM_Data.femalePriority = 5;
                    newArmorAddon.DNAM_Data.EnableWeightSliderMale = item.Value.male.EnableWeightSlider;
                    newArmorAddon.DNAM_Data.EnableWeightSliderFemale = item.Value.female.EnableWeightSlider;

                    bool useDefaultRaces = true;
                    if (keys.Count > 1)
                    {
                        var findRaces = defaultRaces.FindAll(d => d.GetEditorId().StartsWith(keys[1], StringComparison.OrdinalIgnoreCase));
                        if (findRaces != null && findRaces.Count > 0)
                        {
                            newArmorAddon.Race = findRaces.Find(d => !d.GetEditorId().ToLower().Contains("vampire")).GetFormId();
                            useDefaultRaces = false;
                        }
                    }

                    if (useDefaultRaces) {
                        newArmorAddon.Race = "00000019";
                        newArmorAddon.AddDefaultRaces();
                    }
                    if (!newArmors.TryGetValue(key, out Armor newArmor))
                    {
                        newArmor = Armors.AddNewItem(editorID + "AO");
              
                        newArmor.bipedBodyTemplate.FirstPersonFlags = newArmorAddon.bipedBodyTemplate.FirstPersonFlags;
                        newArmor.bipedBodyTemplate.ArmorType = newArmorAddon.bipedBodyTemplate.ArmorType;
                        newArmor.FULLName = NameSet + " " + item.Key.Capitalize();
                        newArmor.Race = newArmorAddon.Race;
                        newArmor.LoadDefaultSettring();
                        newArmors.Add(key, newArmor);
                    }
                    newArmor.SetModelAssets(true, item.Value.male);
                    newArmor.SetModelAssets(false, item.Value.female);
                    newArmor.armatures.Add(newArmorAddon);

                    var alternateTexturesArmorAddons = newArmorAddon.DuplicateByShapeDiffuse(out string[] ATtags, "femalebody_1", "malebody_1");
                    for (int i = 0; i < alternateTexturesArmorAddons.Count; i++)
                    {
                        var ataaddon = alternateTexturesArmorAddons[i];
                        if (!AlternateTexturesArmorAddonSets.TryGetValue(ATtags[i], out List<ArmorAddon> ATaddons))
                        {
                            ATaddons = new List<ArmorAddon>();
                            AlternateTexturesArmorAddonSets.Add(ATtags[i], ATaddons);
                        }
                        ATaddons.Add(ataaddon);
                        var ATKey = key + ATtags[i];
                        
                        if (!AlternateTexturesNewArmors.TryGetValue(ATKey, out Armor alternateTexturesnewArmor))
                        {
                            alternateTexturesnewArmor = (Armor)newArmor.Duplicate(editorID + ATtags[i] + "AO");
                            alternateTexturesnewArmor.armatures.Clear();
                            alternateTexturesnewArmor.FULLName = string.Join(" ", NameSet, item.Key.Capitalize(), ATtags[i]);
                            alternateTexturesnewArmor.CopyAlternateTextureSets(ataaddon.MaleWorldModel, ataaddon.FemaleWorldModel);
                            AlternateTexturesNewArmors.Add(ATKey, alternateTexturesnewArmor);
                        }
                        alternateTexturesnewArmor.armatures.Add(ataaddon);
                    }
                }
            }
            catch (Exception error)
            {
                ("Adding records encountering Exception ->\n"+error.Message).PopupError();
                throw;
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
            if (newArmorAddons.Count > 1) {
                var armorAll = AddArmorByArmaturesFromArmorAddons(NameSet + "AllAA", newArmorAddons);
                armorAll.FULLName = NameSet + " All";
                armorAll.Race = "00000019";
            }
            foreach (var item in AlternateTexturesArmorAddonSets)
            {
                if (item.Value == null || item.Value.Count == 0 || item.Value.Count == 1) continue;
                var ATArmorAll = AddArmorByArmaturesFromArmorAddons(NameSet + item.Key + "AllAA", item.Value);
                ATArmorAll.FULLName = string.Join(" ", NameSet, item.Key, "All");
                ATArmorAll.Race = "00000019";
            }
        }
        public ArmorAddon AddArmorAddonByNifFile(string nifPath, string Nameprfix = "", bool CheckExists = true)
        {
            var nifPathInfo = new PathInfo(nifPath);
            var nifDir = new PathInfo(nifPathInfo.directory);
            var model1stpersonPath = nifDir.CombineWith("1stperson" + nifPathInfo.FileName);
            if (InformationViewer.IsFileExistElsePopup(nifPath))
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
                            ("Model has already been used by " + usedArmorAddon.ToString()).PopupWarnning();
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
                string ArmorAddonName = Nameprfix.MakeValidEditorID() + ArmorAddon.MakeArmorAddonName(editorID);

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
        public Armor AddArmorByArmaturesFromArmorAddons(IEnumerable<ArmorAddon> armorAddons)
                                            => AddArmorByArmaturesFromArmorAddons("", armorAddons);
        public Armor AddArmorByArmaturesFromArmorAddons(string newArmorID, IEnumerable<ArmorAddon> armorAddons) {

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
                        ("Armatures already exists.Which is " + AO.ToString()).PopupWarnning();
                        return AO;
                    }
                }
            }
            var firstArmorAddon = armorAddons.First();
            newArmorID = newArmorID == "" ? firstArmorAddon.EditorID.TrimAfterLast("AA") + "AO" : newArmorID;
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
                var gndModelPath = modelPathInfo.ChangeName(modelPathInfo.Name.TrimAfter("_") + "gnd");
                gndModelPath.PopupInfo();
            }
            return newArmor;
        }
        //public List<TextureSet> AddTextureSetsBySimilarDiffuses(string fileName, int shapeIndex)
        //{
        //    var shapeTextures = NifUtil.GetShapeTexturesArrayByIndex(fileName, shapeIndex);
        //    return AddTextureSetsByDifuseAssets(shapeTextures,
        //        TextureSet.FindSimilarDiffuseTextures(Plugin.GetTexturesPath(shapeTextures[0])));
        //}

        public static List<int> GetBodyPartsIndicesFromNif(string path) {
            List<int> bodyPartsIndices = new List<int>();
            var shapesPartitions = NifUtil.GetBSDismemberBodyParts(path).GetLines();
            foreach (var item in shapesPartitions)
            {
                var infos = item.Split(':');
                infos[infos.Length - 1].Split(',').ToList().ForEach(obj =>
                  {
                      if (int.TryParse(obj, out int bsdBodyPartIndex))
                          if (!bodyPartsIndices.Contains(bsdBodyPartIndex)) bodyPartsIndices.Add(bsdBodyPartIndex);
                  });
            }
            bodyPartsIndices.Sort();
            return bodyPartsIndices;
        }
        public static Setup.GameMode? AssetsFromWhichGame(string path) {
            
            foreach (var item in Enum.GetValues(typeof(Setup.GameMode)))
            {
                var curMod = (Setup.GameMode)item;
                var gamePath = Setup.GetGamePath(curMod);
                if (!gamePath.IsNullOrEmpty()) {
                    if (path.StartsWith(gamePath, StringComparison.OrdinalIgnoreCase))
                        return curMod;
                }
            }
            return null;
        }
    }
}
