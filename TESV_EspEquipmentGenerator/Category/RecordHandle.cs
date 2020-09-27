using System;
using System.Windows.Controls;
using XeLib;
using XeLib.API;

//---------------------------- Work in progress --------------------------------
namespace TESV_EspEquipmentGenerator
{
    public class RecordHandle : Handle
    {
        public Elements.ElementTypes recordType;
        public RecordHandle(uint h) : base(h) {}
        public static implicit operator bool(RecordHandle handle) => handle != null;

        public TreeViewItem GetTreeNode()
        {
            var result = new TreeViewItem();
            //if (this.getele)
            return result;
        }
    }
}
