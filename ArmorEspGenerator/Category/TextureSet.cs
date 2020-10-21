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
        public static string Signature => "TXST";
        public override string signature => Signature;
        public TexturesRGBA texturesRGBA;

        TextureSet(PluginRecords<TextureSet> container, Handle target) : base(container, target)
        {
            texturesRGBA = new TexturesRGBA(target, target.GetElement(TexturesRGBA.Signature));
        }

        public static TextureSet Create(PluginRecords<TextureSet> container, Handle handle)
        {
            if (handle.CompareSignature<TextureSet>()) return new TextureSet(container, handle);
            else return null;
        }        
        public static string[] DisplayNames => new string[]{
             "Difuse",
             "Normal/Gloss",
             "Environment Mask/Subsurface Tint",
             "Glow/Detail Map",
             "Height",
             "Environment",
             "Multilayer",
             "Backlight Mask/Specular"
        };
        public string Difuse { get => texturesRGBA.Difuse; set => texturesRGBA.Difuse = value; }
        public string Normal { get => texturesRGBA.Normal; set => texturesRGBA.Normal = value; }
        public string EnvironmentMask { get => texturesRGBA.EnvironmentMask; set => texturesRGBA.EnvironmentMask = value; }
        public string Detail        { get => texturesRGBA.Detail; set => texturesRGBA.Detail = value; }
        public string High          { get => texturesRGBA.High; set => texturesRGBA.High = value; }
        public string Environment   { get => texturesRGBA.Environment; set => texturesRGBA.Environment = value; }
        public string Multilayer    { get => texturesRGBA.Multilayer; set => texturesRGBA.Multilayer = value; }
        public string Specular { get => texturesRGBA.Specular; set => texturesRGBA.Specular = value; }

        public string this[int index] {
            get => texturesRGBA[index];
            set => texturesRGBA[index] = value;
        }

        public void CopyTexturePath(string[] textures, params int[] ignoreIndices) {
            List<int> indices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            foreach (var index in ignoreIndices) indices.RemoveAt(indices.FindIndex(index));
            foreach (var index in indices) this[index] = textures[index];
        }
        public static string[] FindSimilarDiffusesFromNif(string nifFile)
        {
            var result = new string[0];
            var shapeNames = NifUtil.GetShapeNames(nifFile);
            List<string> diffuses = new List<string>();
            foreach (var item in shapeNames)
            {
                //nifFile.get
            }

            return result;
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
        public string[] ToArray() => texturesRGBA.ToArray();
    }
    public class TexturesRGBA : RecordObject
    {
        public static string Signature => @"Textures (RGB/A)";
        public override string signature => Signature;

        public string GetTextureKey(int index) => @"TX0" + index.ToString();

        public void SetTexture(int index, string value)
        {
            var key = GetTextureKey(index);
            if (index < 0 || index > 7) throw new OutOfMemoryException();
            if (value == string.Empty) handle.Delete(key);
            else value = Plugin.TrimTexturesPath(value);
            PrepareHandle();
            SetValue(key, value);
        }
        string GetTexture(int index)
        {
            if (index < 0 || index > 7) throw new OutOfMemoryException();
            return GetValue(GetTextureKey(index));
        }

        public string Difuse { get => GetTexture(0); set => SetTexture(0, value); }
        public string Normal { get => GetTexture(1); set => SetTexture(1, value); }
        public string EnvironmentMask { get => GetTexture(2); set => SetTexture(2, value); }
        public string Detail { get => GetTexture(3); set => SetTexture(3, value); }
        public string High { get => GetTexture(4); set => SetTexture(4, value); }
        public string Environment { get => GetTexture(5); set => SetTexture(5, value); }
        public string Multilayer { get => GetTexture(6); set => SetTexture(6, value); }
        public string Specular { get => GetTexture(7); set => SetTexture(7, value); }

        public string this[int index]
        {
            get => GetTexture(index);
            set => SetTexture(index, value);
        }
        public TexturesRGBA(Handle Parent, Handle target) : base(Parent, target) {}

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
    }

}
