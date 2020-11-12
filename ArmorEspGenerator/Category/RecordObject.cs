using System;
using System.IO;
using System.Collections.Generic;
using XeLib.API;
using XeLib;
using Yu5h1Tools.WPFExtension;
using System.Windows.Controls;

namespace TESV_EspEquipmentGenerator
{
    public interface IRecordObject {
        string signature { get; }
        Handle parent { get; }
        Handle handle { get; }
        string DisplayName { get; }
        void Delete();
    }
    public interface IRecordArrayObject : IRecordObject
    {
        void Clear();
    }
    public abstract class RecordObject : IRecordObject
    {
        public const string FullNameKEY = "FULL - Name";
        public static string RaceKey => "RNAM - Race";
        public abstract string signature { get; }
        public Handle parent { get; protected set; }
        public virtual Handle handle { get; protected set; }
        public virtual string DisplayName => handle.GetDisplayName();
        public static implicit operator bool(RecordObject obj) => obj != null;
        public string GetValue(string path) => handle.GetValue(path);
        public void SetValue(string path, string value) => handle.SetValue(path, value);
        public RecordObject(Handle Parent,Handle target = null)
        {
            parent = Parent;
            handle = target == null ? Parent.GetElement(signature) : target;
        }
        public void PrepareHandle()
        {
            if (handle == null) {
                if (parent == null) (GetType().Name + " parent is null").PromptWarnning();
                else handle = parent.AddElement(signature);
            } 
        }
        public virtual void Delete() => handle.Delete();
    }
    public abstract class RecordArrays<T> : List<T>, IRecordArrayObject
    {
        public virtual string signature { get; }
        public virtual Handle parent { get; protected set; }
        public virtual Handle handle => parent.GetElement(signature);

        public string DisplayName => handle.GetDisplayName();

        public static implicit operator bool(RecordArrays<T> obj) => obj != null;
        public RecordArrays(Handle Parent) {
            parent = Parent;            
        }
        public virtual bool PrepareHandle()
        {
            if (handle == null)
            {
                if (parent == null) (GetType().Name + " parent is null").PromptWarnning();
                else
                {
                    parent.AddElement(signature);
                    return true;
                }
            }
            return false;
        }
        public new void Add(T item)
        {
            PrepareHandle();
            base.Add(item);
        }
        public virtual void Delete() => handle.Delete();

    }
    public abstract class RecordElement : RecordObject
    {        
        public static Handle SkyrimESM => Handle.BaseHandle.GetElement("Skyrim.esm");
        public string FormID => handle.GetFormID();
        public string EditorID { get => handle.GetEditorID(); set => handle.SetEditorID(value); }
        public string RecordHeaderFormID => handle.GetRecordHeaderFormID();
        public RecordElement(Handle Parent,Handle target) : base(Parent, target) { }
        public override string ToString() => handle.GetRecordHeaderFormID();
        public virtual string GetDataInfo() => "";
    }
    public interface IRecordElement
    {
       object Duplicate(string newEditorID = "");
    }
    public abstract class RecordElement<T> : RecordElement, IRecordElement where T : RecordElement<T>
    {
        public PluginRecords<T> Container;
        public Plugin plugin => Container.plugin;
        public RecordElement(PluginRecords<T> container, Handle target) : base(container.handle, target) {
            Container = container;
        }
        public object Duplicate(string newEditorID = "") => Container.Duplicate((T)this, newEditorID);
        public override void Delete() {
            handle.Delete();
            Container?.Remove((T)this);
        }
    }
}
