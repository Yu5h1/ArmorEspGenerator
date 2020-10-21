using System;
using System.Linq;
using System.Collections.Generic;
using XeLib;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public class PluginRecords<T> : List<T> where T : RecordElement<T>
    {
        public Plugin plugin;
        public Handle handle;
        public Action<T> Added;
        Func<PluginRecords<T>, Handle, T> Constructor;

        public PluginRecords(Plugin plugin, Func<PluginRecords<T>, Handle, T> constructor)
        {
            this.plugin = plugin;
            Constructor = constructor;
            foreach (var item in plugin.handle.GetElements())
            {
                if (item.GetSignature() == SignatureUtil.GetSignature<T>()) {
                    handle = item;
                    break;
                }
            }
            foreach (var item in plugin.GetRecords()) Add(item);
        }
        public new void Add(T item)
        {
            base.Add(item);
            Added?.Invoke(item);
        }
        public T Add(Handle handle)
        {
            T result = null;
            if (handle.CompareSignature<T>())
            {
                result = Constructor(this, handle);
                Add(result);
            }
            return result;
        }

        public T AddNewItem(string editorID = "")
        {
            if (editorID.Equals(string.Empty)) editorID = ("New" + typeof(T).Name);
            editorID = editorID.MakeValidEditorID().GetUniqueStringWithSuffixNumber(this, d => d.EditorID);
            T result = Constructor(this, handle.AddElement(SignatureUtil.GetSignature<T>()));
            result.EditorID = editorID;
            Add(result);
            return result;
        }
        public T Duplicate(T source, string newEditorID = "")
        {
            if (newEditorID == "") newEditorID = source.EditorID;
            newEditorID = newEditorID.GetUniqueStringWithSuffixNumber(this, d => d.EditorID);
            return Add(source.handle.CopyAsNew(newEditorID, plugin.handle));
        }

        public new void Remove(T item)
        {
            if (Exists(d => d.Equals(item)))
            {
                item.handle.Delete();
                base.Remove(item);
            }
        }
        public new void RemoveAt(int index)
        {
            if (index > -1 && index < Count)
            {
                this[index].handle.Delete();
                base.RemoveAt(index);
            }
        }
        public new void Clear()
        {
            foreach (var item in this) item.Delete();
            base.Clear();
        }
    }

}
