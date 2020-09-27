using System;
using System.IO;
using System.Collections.Generic;
using XeLib.API;
using XeLib;
using Yu5h1Tools.WPFExtension;
using System.Windows.Controls;

namespace TESV_EspEquipmentGenerator
{
    public abstract class RecordObject 
    {
        public Handle handle { get; protected set; }
        public static implicit operator bool(RecordObject obj) => obj != null;
    }
    public abstract class RecordElement : RecordObject
    {        
        public static Handle SkyrimESM => Handle.BaseHandle.GetElement("Skyrim.esm");
        
        public string GetValue(string path) => handle.GetValue(path);
        public void SetValue(string path, string value) => handle.SetValue(path, value);

        public string FormID => handle.GetFormID();
        public string EditorID { get => handle.GetEditorID(); set => handle.SetEditorID(value); }

        public abstract string signature { get; }


        

        public RecordElement(Handle target)
        {
            handle = target;
        }
        public virtual void Delete() => handle.Delete();

        public override string ToString() => handle.GetLabel();
        public virtual string GetDataInfo() => "";
    }
    public abstract class RecordElement<T> : RecordElement where T : RecordElement<T>
    {
        public PluginRecords<T> Container;
        public Plugin plugin => Container.plugin;
        public abstract string templateEditorID { get; }
        public RecordElement(PluginRecords<T> container, Handle target) : base(target) {
            Container = container;
        }
        public override void Delete() =>
            Container.RemoveAt(Container.FindIndex( d=>d.Equals(this)));
        public abstract void Clean();
    }
}
