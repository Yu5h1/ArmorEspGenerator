using System.IO;
using System.Collections.Generic;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public class WorldModel : RecordObject
    {
        public bool MaleOrFemale;
        public static string Signature(bool maleOrfemale)
                             => (maleOrfemale ? "Male" : "Female") + " world model";
        public override string signature => Signature(MaleOrFemale);

        public string modelKey => handle.GetSignatureByDisplayName("Model");
        public string alternateTexturesKey => handle.GetSignatureByDisplayName("Alternate Textures");

        public List<string> ShapesNames { get; private set; }
        public string Model
        {
            get => handle.GetValue(modelKey);
            set
            {
                if (value != Model)
                {
                    if (Plugin.IsLocateAtGameAssetsFolder(value))
                    {
                        if (Plugin.ContainMeshesFolderInPath(value))
                            value = Plugin.TrimMeshesPath(value);
                    }
                    PrepareHandle();
                    handle.SetValue(modelKey, value);
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
            string[] shapesPathes = new string[0];
            var fullMeshPath = Plugin.GetMeshesPath(Model);
            if (File.Exists(fullMeshPath)) shapesPathes = NifUtil.GetShapeNames(fullMeshPath);
            ShapesNames = new List<string>(shapesPathes);
        }


        public WorldModel(RecordObject parentObj, bool maleOrfemale) :
                     base(parentObj.handle, parentObj.handle.GetElement(Signature(maleOrfemale)))
        {
            MaleOrFemale = maleOrfemale;
            UpdateShapesNames();
            alternateTextures = new AlternateTextures(this);
        }
        public override string ToString() => Model + '\n' + alternateTextures.ToString();
    }
}
