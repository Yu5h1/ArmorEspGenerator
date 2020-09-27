using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using XeLib;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public class TextureSet : RecordElement<TextureSet>
    {
        public const string Signature = "TXST";
        public static string TemplateEditorID = "00000889";

        public override string signature => Signature;
        public override string templateEditorID => TemplateEditorID;
        TextureSet(PluginRecords<TextureSet> container, Handle target) : base(container, target) { }

        public static TextureSet Create(PluginRecords<TextureSet> container, Handle handle)
        {
            if (handle.CompareSignature<TextureSet>()) return new TextureSet(container, handle);
            else return null;
        }
        public string texturekey = @"Textures (RGB/A)\TX0";
        public void SetTexture(int index, string value) {
            var key = texturekey + index.ToString();
            if (index < 0 || index > 7) throw new OutOfMemoryException();
            if (value == string.Empty) handle.Delete(key);
            else if (Plugin.ContainTexturesFolderInPath(value)) {
                if (Plugin.IsLocateAtGameAssetsFolder(value))
                    SetValue(key, Plugin.TrimTexturesPath(value));
            }else SetValue(key, value);
        }
        string GetTexture(int index)
        {
            if (index < 0 || index > 7) throw new OutOfMemoryException();
            return GetValue(texturekey + index.ToString());
        }
        public static string[] Names => new string[]{
            "Difuse",
            "Normal",
            "Environment",
            "Detail",
            "High",
            "Environment",
            "Multilayer",
            "Specular",
        };
        public string Difuse { get => GetTexture(0); set => SetTexture(0, value); }
        public string Normal { get => GetTexture(1); set => SetTexture(1, value); }
        public string EnvironmentMask { get => GetTexture(2); set => SetTexture(2, value); }
        public string Detail { get => GetTexture(3); set => SetTexture(3, value); }
        public string High { get => GetTexture(4); set => SetTexture(4, value); }
        public string Environment { get => GetTexture(5); set => SetTexture(5, value); }
        public string Multilayer { get => GetTexture(6); set => SetTexture(6, value); }
        public string Specular { get => GetTexture(7); set => SetTexture(7, value); }

        public string this[int index] {
            get => GetTexture(index);
            set => SetTexture(index, value);
        }

        public void CopyTexturePath(string[] textures, params int[] ignoreIndices) {
            List<int> indices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            foreach (var index in ignoreIndices) indices.RemoveAt(indices.FindIndex(index));
            foreach (var index in indices) this[index] = textures[index];
        }
        public static string[] FindSimilarDiffuseTextures(string difuse)
        {
            var result = new string[0];
            if (File.Exists(difuse))
            {
                string tag = Path.GetFileNameWithoutExtension(difuse).RemoveSuffixFrom("_");
                result = Directory.GetFiles(Path.GetDirectoryName(difuse), tag + "*.dds").Where(d => !d.Contains("_n", "_s") && !string.Equals(d, difuse, StringComparison.OrdinalIgnoreCase)).ToArray();
            }
            return result;
        }
        public string[] ToArray() => new string[] {
            Difuse ,
            Normal ,
            EnvironmentMask,
            Detail ,
            High ,
            Environment ,
            Multilayer ,
            Specular
        };
        public override void Clean(){
            for (int i = 0; i < 8; i++) this[i] = "";
        }
    }
}
