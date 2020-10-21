﻿using System;
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
    public abstract class RecordObject : IRecordObject
    {        
        public abstract string signature { get; }
        public Handle parent { get; protected set; }
        public virtual Handle handle { get; protected set; }
        public string DisplayName => handle.GetDisplayName();
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
            if (handle == null) handle = parent.AddElement(signature);
        }
        public virtual void Delete() => handle.Delete();
    }
    public abstract class RecordArrayObject<T> : List<T>,IRecordObject
    {
        public virtual string signature { get; }
        public Handle parent { get; protected set; } 
        public virtual Handle handle => parent.GetElement(signature);

        public string DisplayName => handle.GetDisplayName();

        public static implicit operator bool(RecordArrayObject<T> obj) => obj != null;
        public RecordArrayObject(Handle Parent) => parent = Parent;
        public new void Add(T item)
        {
            if (handle == null) parent.AddElement(signature);
            base.Add(item);
        }
        public virtual void Delete() => handle.Delete();
    }
    public abstract class RecordElement : RecordObject
    {        
        public static Handle SkyrimESM => Handle.BaseHandle.GetElement("Skyrim.esm");

        public string FormID => handle.GetFormID();
        public string EditorID { get => handle.GetEditorID(); set => handle.SetEditorID(value); }

        public RecordElement(Handle Parent,Handle target) : base(Parent, target) { }
        public override string ToString() => handle.GetLabel();
        public virtual string GetDataInfo() => "";
    }
    public interface IRecordElement
    {
       void Duplicate();
    }
    public abstract class RecordElement<T> : RecordElement, IRecordElement where T : RecordElement<T>
    {
        public PluginRecords<T> Container;
        public Plugin plugin => Container.plugin;
        public RecordElement(PluginRecords<T> container, Handle target) : base(container.handle, target) {
            Container = container;
        }
        public void Duplicate() => Container.Duplicate((T)this); 

        public override void Delete() => Container.RemoveAt(Container.FindIndex( d=>d.Equals(this)));
    }
}
