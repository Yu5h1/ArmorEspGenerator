using System.Linq;
using System.Collections.Generic;
using Yu5h1Tools.WPFExtension;
using XeLib;
using XeLib.API;

namespace TESV_EspEquipmentGenerator
{
    public class AlternateTextures : RecordArrayObject<AlternateTexture>
    {
        public override string signature => worldModel.alternateTexturesKey;

        public ModelAlternateTextures worldModel { get; private set; }
        public List<string> ShapesNames => worldModel.ShapesNames;
        public List<Handle> AlternateTexturesHandles => handle.GetArrayItems("Alternate Texture").ToList();
        public AlternateTextures(ModelAlternateTextures worldModel) : base(worldModel.handle)
        {
            this.worldModel = worldModel;
            foreach (var item in AlternateTexturesHandles) Add(new AlternateTexture(this, item));
            //Sort((a, b) => a.ShapeIndex.CompareTo(b.ShapeIndex));
        }
        public void Set(string shapeName, TextureSet textureSet) => Set(shapeName, textureSet.handle);

        public void Set(string shapeName, Handle textureSet)
        {
            
            if (handle == null) parent.AddElement(signature);
            var element = Find(a => a.ShapeName == shapeName);
            if (element == null)
            {
                Add(new AlternateTexture(this, handle.AddArrayItem(), shapeName, textureSet));
            }
            else element.NewTexture = textureSet.GetFormID();
            //Sort();
        }
        //public new void Sort()
        //{
        //    if (AlternateTexturesHandles.Count < 2) return;

        //    bool needSort = false;
        //    for (int i = 0; i < Count-1; i++)
        //    {
        //        if (this[i].ShapeIndex > this[i + 1].ShapeIndex) {
        //            needSort = true;
        //            break;
        //        }
        //    }
        //    if (needSort) {
        //        "Need sort AlternateTextures".PromptInfo();
        //        var datas = AlternateTexturesHandles;
        //        datas.Count.PromptInfo();
        //        for (int i = 0; i < datas.Count-1; i++)
        //        {
        //            var cur = datas[i];
        //            var next = datas[i + 1];
        //            int curIndex = AlternateTexture.Get3DIndex(cur);
        //            int nextIndex = AlternateTexture.Get3DIndex(next);
        //            if (curIndex > nextIndex) {
        //                string cur3DName = AlternateTexture.Get3DName(cur);
        //                string curNewTexutre = AlternateTexture.GetNewTexture(cur);
        //                string next3DName = AlternateTexture.Get3DName(next);
        //                string nextNewTexture = AlternateTexture.GetNewTexture(next);

        //                AlternateTexture.Set3DIndex(cur, nextIndex);
        //                AlternateTexture.Set3DName(cur, next3DName);
        //                AlternateTexture.SetNewTexture(cur, nextNewTexture);
        //                AlternateTexture.Set3DIndex(next, curIndex);
        //                AlternateTexture.Set3DName(next, cur3DName);
        //                AlternateTexture.SetNewTexture(next, curNewTexutre);
        //            }
        //        }
        //        datas.Select(d => d.GetValue("3D Index")).ToArray().ToContext().PromptInfo();
        //    }
        //}
        public void Remove(string shapeName)
        {
            RemoveAt(FindIndex(d => d.ShapeName == shapeName));
        }
        public new void Remove(AlternateTexture alternateTexture) => RemoveAt(FindIndex(d => d == alternateTexture));
        public new void RemoveAt(int index)
        {
            if (index >= 0 && index < Count) {
                this[index].handle.Delete();
                base.RemoveAt(index);
            }
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
        public static string Signature => "Alternate Texture";
        public override string signature => Signature;
        public AlternateTextures container { get; private set; }
        public List<string> ShapesNames => container.ShapesNames;
        public AlternateTexture(AlternateTextures alternateTextures, Handle target, string shapeName = "", Handle textureSet = null) :
            base(alternateTextures.handle, target)

        {
            container = alternateTextures;
            if (shapeName != "") ShapeName = shapeName;
            if (textureSet != null) NewTexture = textureSet.GetRecordHeaderFormID();
        }
        public string ShapeName
        {
            get => handle.GetValue("3D Name");
            set
            {
                int index = ShapesNames.IndexOf(value);
                if (index > -1)
                {
                    handle.SetValue("3D Name", value);
                    if (ShapeIndex != index) ShapeIndex = index;
                }
            }
        }
        public string NewTexture
        {
            get => handle.GetValue("New Texture");
            set
            {
                if (value == null || value == string.Empty) Delete();
                else handle.SetValue("New Texture", value);
            }
        }
        public int ShapeIndex
        {
            get => handle.GetInteger("3D Index");
            set
            {
                if (value > -1 && value < ShapesNames.Count)
                {
                    handle.SetInteger("3D Index", value);
                    if (ShapeName != ShapesNames[value]) ShapeName = ShapesNames[value];
                }
            }
        }
        public static int Get3DIndex(Handle handle) => handle.GetInteger("3D Index");
        public static string Get3DName(Handle handle) => handle.GetValue("3D Name");
        public static string GetNewTexture(Handle handle) => handle.GetValue("New Texture");
        public static void Set3DIndex(Handle handle, int value) => handle.SetInteger("3D Index", value);
        public static void Set3DName(Handle handle, string value) => handle.SetValue("3D Name", value);
        public static void SetNewTexture(Handle handle, string value) => handle.SetValue("New Texture", value);


        public override void Delete() => container.Remove(this);
        public override string ToString() => ShapeIndex.ToString() + " . " + ShapeName + " : " + NewTexture;
    }
}
