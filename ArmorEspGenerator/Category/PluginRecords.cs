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
        public Action<T> Added;
        Func<PluginRecords<T>, Handle, T> Constructor;

        public PluginRecords(Plugin plugin, Func<PluginRecords<T>, Handle, T> constructor)
        {
            this.plugin = plugin;
            Constructor = constructor;
            foreach (var item in plugin.GetRecords()) Add(item);
        }
        public bool Add(Handle handle)
        {
            if (handle.CompareSignature<T>())
            {
                Add(Constructor(this, handle));
                return true;
            }
            
            return false;
        }
        public T AddNewItem(string editorID, T source = null)
        {
            
            if (source == null) {
                source = Constructor(this, RecordElement.SkyrimESM.GetElement(SignatureUtil.GetTemplateEditorID<T>()));
                source.Clean();
            }
            if (source != null)
            {
                T result = Constructor(this, source.handle.CopyAsNew(editorID, plugin.handle));
                Add(result);
                Added?.Invoke(result);
                return result;
            }
            return null;
        }
        public void Duplicate(T item) {
            AddNewItem(item.EditorID.GetUniqueStringWithSuffixNumber(this,d=>d.EditorID), item);
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
