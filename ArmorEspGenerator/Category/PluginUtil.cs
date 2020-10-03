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
            int index = value.ToLower().IndexOf(folder.ToLower());
            if (index >= 0) value = value.Substring(index + folder.Length).RemovePrefixUtilEmpty(@"\");
            return value;
        }
        public static string TrimTexturesPath(string value) => TrimSourceFolderPath(value, GetTexturesPath());
        public static string TrimMeshesPath(string value) => TrimSourceFolderPath(value, GetMeshesPath());

        public List<TextureSet> AddTextureSetsByDifuseAssets(string[] defaultTexturesPath, params string[] files)
        {
            var results = new List<TextureSet>();
            foreach (var path in files)
            {
                if (!IsLocateAtGameAssetsFolder(path)) return null;
                
                var DifusePath = TrimTexturesPath(path);
                var existsTextureSet = TextureSets.Find(d => d.Difuse.Equals(DifusePath, StringComparison.OrdinalIgnoreCase));
                if (existsTextureSet == null)
                {
                    existsTextureSet = TextureSets.AddNewItem(Path.GetFileNameWithoutExtension(path));
                    existsTextureSet.Difuse = DifusePath;
                }
                if (defaultTexturesPath != null)
                    if (defaultTexturesPath.Length > 7)
                        existsTextureSet.CopyTexturePath(defaultTexturesPath, 0);

                if (existsTextureSet) results.Add(existsTextureSet);
                else "Create New TextureSet Failed ! ".PromptWarnning();
            }
            return results;
        }
        public void GenerateEspByFolder(string shapeName)
        {

        }
        public void GenerateArmorByFolder(string FolderPath)
        {
            var nifFiles = Directory.GetFiles(FolderPath, "*_1.nif");

        }
        public void GenerateArmorByNifFile(string nif)
        {
            var nifPathInfo = new PathInfo(nif);
            Armors.AddNewItem(nifPathInfo.Name.RemoveSuffixFrom("_1"));

        }
        public static List<int> GetBodyPartsIDsFromNif(string path) {
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
