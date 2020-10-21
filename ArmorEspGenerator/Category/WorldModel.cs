using System.IO;
using System.Collections.Generic;
using Yu5h1Tools.WPFExtension;
using XeLib;

namespace TESV_EspEquipmentGenerator
{
    public class WorldModel : RecordObject
    {
        public bool? MaleOrFemale = null;
        public static string Signature(bool? maleOrfemale)
        {
            if (maleOrfemale == null) return "";
            return (((bool)maleOrfemale) ? "Male" : "Female") + " world model";
        }
        public override string signature => Signature(MaleOrFemale);

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
        public string[] GetShapeTextures(int shapeIndex) => NifUtil.GetShapeTextures(FullModelPath, ShapesNames[shapeIndex]).GetLines();

        public List<TextureSet> AddTextsetsBySimilarDiffuses(Plugin plugin, int shapeIndex)
        {
            var shapeTextures = GetShapeTextures(shapeIndex);
            return plugin.AddTextureSetsByDifuseAssets(shapeTextures,
                TextureSet.FindSimilarDiffuseTextures(Plugin.GetTexturesPath(shapeTextures[0])));
        }
        void UpdateShapesNames()
        {
            ShapesNames.Clear();
            if (string.IsNullOrEmpty(Model)) return;
            string[] shapesPathes = new string[0];
            var fullMeshPath = Plugin.GetMeshesPath(Model);
            if (File.Exists(fullMeshPath)) shapesPathes = NifUtil.GetShapeNames(fullMeshPath);
            ShapesNames = new List<string>(shapesPathes);
        }


        public WorldModel(Handle parent, bool maleOrfemale) : 
                     base(parent, parent.GetElement(Signature(maleOrfemale)))
        {
            MaleOrFemale = maleOrfemale;
            UpdateShapesNames();
            alternateTextures = new AlternateTextures(this);
        }
        public override string ToString() => Model + '\n' + alternateTextures.ToString();
    }
}
