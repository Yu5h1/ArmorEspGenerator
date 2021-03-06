﻿using System.IO;
using System.Collections.Generic;
using Yu5h1Tools.WPFExtension;
using XeLib;

namespace TESV_EspEquipmentGenerator
{
    public class ModelAlternateTextures : RecordObject
    {
        public bool? IsFirstPerson = null;
        public bool? MaleOrFemale = null;
        public static string Signature(bool? maleOrfemale,bool? isFirstPerson)
        {
            if (isFirstPerson == null || maleOrfemale == null) return "";
            return (((bool)maleOrfemale) ? "Male" : "Female") + (((bool)isFirstPerson) ? " 1st Person" : " world model");
        }
        public override string signature => Signature(MaleOrFemale, IsFirstPerson);

        public string modelKey => handle.GetSignatureByDisplayName("Model");
        public string alternateTexturesKey => handle.GetSignatureByDisplayName("Alternate Textures");

        public List<string> ShapesNames { get; private set; } = new List<string>();
        public string Model
        {
            get => handle.GetValue(modelKey);
            set
            {
                if (value != Model)
                {
                    PrepareHandle();
                    handle.SetValue(modelKey, Plugin.TrimMeshesPath(value));
                    UpdateShapesNames();
                }
            }
        }
        public AlternateTextures alternateTextures;

        public string FullModelPath => Model == "" ? "" : Plugin.GetMeshesPath(Model);
        void UpdateShapesNames()
        {
            ShapesNames.Clear();
            if (string.IsNullOrEmpty(Model)) return;
            string[] shapesPathes = new string[0];
            var fullMeshPath = Plugin.GetMeshesPath(Model);
            if (File.Exists(fullMeshPath)) shapesPathes = NifUtil.GetShapeNames(fullMeshPath);
            ShapesNames = new List<string>(shapesPathes);
        }
        public ModelAlternateTextures(Handle parent, bool maleOrfemale,bool isFirstPerson = false) : 
                     base(parent, parent.GetElement(Signature(maleOrfemale, isFirstPerson)))
        {
            MaleOrFemale = maleOrfemale;
            IsFirstPerson = isFirstPerson;
            UpdateShapesNames();
            alternateTextures = new AlternateTextures(this);
        }
        public void CopyAlternateTexturesFrom(ModelAlternateTextures data) {
            if (ShapesNames.Count == 0 || Model == "" || data == null || data == this) return;
            alternateTextures.Clear();
            foreach (var item in data.alternateTextures)
            {
                int shapeindex = ShapesNames.IndexOf(item.ShapeName);
                if (shapeindex >= 0) alternateTextures.Set(item);
            }
        }
        public override string ToString() => Model + '\n' + alternateTextures.ToString();
    }
}
