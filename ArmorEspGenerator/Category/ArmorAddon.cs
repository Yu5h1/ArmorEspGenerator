using System;
using System.Collections.Generic;
//using System.Linq;
using System.IO;
using System.Threading.Tasks;
using XeLib.API;
using XeLib;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public class ArmorAddon : RecordElement<ArmorAddon>
    {                
        public const string Signature = "ARMA";
        public static string TemplateEditorID = "00000D67";
        public override string signature => Signature;
        public override string templateEditorID => TemplateEditorID;

        ArmorAddon(PluginRecords<ArmorAddon> container, Handle target) : base(container, target) {
            MaleWorldModel = new WorldModel(this, GetWorldModelKeys(true));
            FemaleWorldModel = new WorldModel(this, GetWorldModelKeys(false));
        }
        public static ArmorAddon Create(PluginRecords<ArmorAddon> container, Handle handle)
        {
            if (handle.CompareSignature<ArmorAddon>()) return new ArmorAddon(container, handle);
            else return null;
        }
        (string ,string ,string ) GetWorldModelKeys(bool MaleOrFemale) => (
                            (MaleOrFemale? "Male":"Female")+" world model",
                            "MOD"+(MaleOrFemale ? "2" : "3" ),
                            "MO"+ (MaleOrFemale ? "2" : "3" ) + "S");

        public Handle FirstPersonFlags => handle.GetElement(@"BOD2\First Person Flags");

        public WorldModel MaleWorldModel;
        public WorldModel FemaleWorldModel;
        public WorldModel GetWorldModel(bool MaleOrFemale) => MaleOrFemale ? MaleWorldModel : FemaleWorldModel;
        public override string GetDataInfo() => ToString()+'\n'+ FemaleWorldModel.ToString();
        public override void Clean()
        {
            throw new NotImplementedException();
        }
    }
    public class WorldModel
    {
        public ArmorAddon armorAddon { get; private set; }
        public (string worldModel, string model, string alternateTexture) KEYS;
        public bool MaleOrFemale => !KEYS.worldModel.Contains("Female");
        public Handle handle => armorAddon.handle.GetElement(KEYS.worldModel);
        public List<string> ShapesNames { get; private set; }
        public string Model {
            get => handle.GetValue(KEYS.model);
            set {
                if (value != Model) {
                    if (handle != null) handle.SetValue(KEYS.model, value);
                    else armorAddon.handle.SetValue(KEYS.worldModel + @"\" + KEYS.model, value);
                    UpdateShapesNames();
                }
            } 
        }
        public string FullModelPath => Model == "" ? "" : Plugin.GetMeshesPath(Model);
        public string[] GetShapeTextures(int shapeIndex) => NifUtil.GetShapeTextures(FullModelPath, ShapesNames[shapeIndex]).GetLines();

        public void BodyPartsToPartitions() {
            PartitionsUtil.SetPartitionFlags(armorAddon.FirstPersonFlags,
                PartitionsUtil.ConvertIndicesToBodyParts(Plugin.GetBodyPartsIDsFromNif(FullModelPath)).BSDismemberBodyPartsToPartitions());
        }

        public List<TextureSet> AddTextsetsBySimilarDiffuses(int shapeIndex) {
            var shapeTextures = GetShapeTextures(shapeIndex);
            return armorAddon.plugin.AddTextureSetsByDifuseAssets(
                shapeTextures,
                TextureSet.FindSimilarDiffuseTextures(Plugin.GetTexturesPath(shapeTextures[0]))
                );
        }
        void UpdateShapesNames() {
            string[] shapesPathes = new string[0];
            var fullMeshPath = Plugin.GetMeshesPath(Model);
            if (File.Exists(fullMeshPath)) shapesPathes = NifUtil.GetShapesNames(fullMeshPath);
            ShapesNames = new List<string>(shapesPathes);
        }
        public AlternateTextures alternateTextures;

        public WorldModel(ArmorAddon armorAddon, (string , string , string ) keys)
        {
            this.armorAddon = armorAddon;
            KEYS = keys;
            UpdateShapesNames();
            alternateTextures = new AlternateTextures(this, KEYS.alternateTexture);
        }
        public override string ToString() => Model + '\n' + alternateTextures.ToString();
    }
    public class AlternateTextures : List<AlternateTexture>
    {
        public WorldModel worldModel { get; private set; }
        public List<string> ShapesNames => worldModel.ShapesNames;
        public PluginRecords<TextureSet> PluginTextureSets => worldModel.armorAddon.plugin.TextureSets;
        public string AlternateTextureKey;
        public Handle handle => worldModel.handle.GetElement(AlternateTextureKey);
        public AlternateTextures(WorldModel worldModel, string alternateTextureKey)
        {
            this.worldModel = worldModel;
            AlternateTextureKey = alternateTextureKey;
            foreach (var item in handle.GetArrayItems("Alternate Texture")) {
                Add(new AlternateTexture(this, item));
            }
            Sort((a, b) => a.ShapeIndex.CompareTo(b.ShapeIndex));
        }
        public void Set(string shapeName, TextureSet textureSet) => Set(shapeName, textureSet.handle);

        public void Set(string shapeName, Handle textureSet)
        {
            if (ShapesNames.Exists(s => s == shapeName))
            {
                var existsElement = Find(a => a.ShapeName == shapeName);
                if (Count == 0)
                {
                    worldModel.handle.SetValue(AlternateTextureKey, "");
                    Elements.AddArrayItem(worldModel.handle, AlternateTextureKey + " - Alternate Textures", "3D Name", shapeName);
                    Add(new AlternateTexture(this, handle.GetElement("Alternate Texture"), shapeName, textureSet));
                }
                else if (existsElement == null)
                    Add(new AlternateTexture(this, Elements.AddArrayItem(handle, "", "", ""), shapeName, textureSet));
                else existsElement.targetTextureSet = textureSet.ToString();
            }
        }
        public void Remove(string shapeName) {
            RemoveAt(FindIndex(d => d.ShapeName == shapeName));
        }
        public new void Remove(AlternateTexture alternateTexture) => RemoveAt(FindIndex( d=>d == alternateTexture));
        public new void RemoveAt(int index)
        {
            if (index < Count) this[index].handle.Delete();
            base.RemoveAt(index);
        }
        public new void Clear()
        {
            foreach (var item in this) item.handle.Delete();
            base.Clear();
        }
        public override string ToString() => string.Join("\n", this);
    }
    public class AlternateTexture : RecordObject
    {
        public List<string> ShapesNames => list.ShapesNames;
        public AlternateTextures list { get; private set; }
        public AlternateTexture(AlternateTextures alternateTextures, Handle handle, string shapeName = "", Handle textureSet = null)
        {
            this.handle = handle;
            this.list = alternateTextures;
            if (shapeName != "") ShapeName = shapeName;
            if (textureSet != null) targetTextureSet = textureSet.GetLabel();
        }
        public string ShapeName {
            get => handle.GetValue("3D Name");
            set {
                int index = ShapesNames.IndexOf(value);
                if (index > -1) {
                    handle.SetValue("3D Name", value);
                    if (ShapeIndex != index) ShapeIndex = index;
                }
            }
        }
        public string targetTextureSet {
            get => handle.GetValue("New Texture");
            set {
                if (value == null || value == string.Empty) Delete();
                else handle.SetValue("New Texture", value.ToString());
            } 
        }
        public int ShapeIndex { get => handle.GetInteger("3D Index");
            set {
                if (value > -1 && value < ShapesNames.Count) {
                    handle.SetInteger("3D Index", value);
                    if (ShapeName != ShapesNames[value]) ShapeName = ShapesNames[value];
                }
            }
        }
        public void Delete() => list.Remove(this);
        public override string ToString() => ShapeIndex.ToString()+" . "+ShapeName + " : " + targetTextureSet;
    }

}
