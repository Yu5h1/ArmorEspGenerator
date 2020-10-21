using System;
using System.IO;
using System.Windows.Controls;
using XeLib;
using XeLib.API;
using Yu5h1Tools.WPFExtension;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using Yu5h1Tools.WPFExtension.CustomControls;
using System.Windows.Media;

namespace TESV_EspEquipmentGenerator
{
    public static class RecordsUI
    {
        public static TreeViewItem GetTreeNode(this Handle handle,string elementName) {
            var elementHandle = handle.GetElement(elementName);
            if (elementHandle == null)
            {
                return new TreeViewItem()
                {
                    Header = new TextBlock {
                        Foreground = Brushes.DarkGray,
                        Text = elementName,
                        IsEnabled = false
                    }
                };
            }
            return elementHandle.GetTreeNode();
        }
        public static TreeViewItem GetTreeNode(this Handle handle)
        {
            var result = new TreeViewItem();
            if (handle.GetValue() != null)
            {
                result.SetControlLabel(handle.GetDisplayName(), new TextBox() {
                    Text = handle.GetValue(),
                    IsEnabled = false
                });
            }else {
                result.SetTextBlockHeader(handle.GetDisplayName());
            }

            result.Tag = handle;
            result.ToolTip = handle.GetElementType().ToString();
            result.IsExpanded = true;

            var definitions = handle.GetDefineNames();
            foreach (var define in definitions)
            {
                var subhandle = handle.GetElement(define);
                TreeViewItem subTreeItem = null;
                if (subhandle == null) {
                    subTreeItem = new TreeViewItem().SetHeader(new TextBlock() {
                        Text = define,
                        Foreground = Brushes.DarkGray
                    });
                }else subTreeItem = subhandle.GetTreeNode();

                result.Items.Add(subTreeItem);
            }
            //var value = handle.GetValue();
            //string valueLabel = value == "" ? "" : " : [" + value + "]";
            
            
    
            //var elements = handle.GetElements();            
            //if (elements.Length > 0)
            //foreach (var element in elements)
            //{
            //    var DisplayName = element.GetDisplayName();
            //    switch (element.GetElementType())
            //    {
            //            //case Elements.ElementTypes.EtFile:
            //            //    break;
            //            case Elements.ElementTypes.EtMainRecord:
            //                //result.Items.Add(GetTreeNode(item));
            //                break;
            //            //case Elements.ElementTypes.EtGroupRecord:
            //            //    break;
            //            case Elements.ElementTypes.EtSubRecord:
            //                result.Items.Add(GetTreeNode(element));
            //                break;
            //            case Elements.ElementTypes.EtSubRecordStruct:
            //                result.Items.Add(GetTreeNode(element));
            //                break;
            //            case Elements.ElementTypes.EtSubRecordArray:
            //                result.Items.Add(GetTreeNode(element));
            //                break;
            //            //case Elements.ElementTypes.EtSubRecordUnion:
            //            //    break;
            //            //case Elements.ElementTypes.EtArray:
            //            //    break;
            //            case Elements.ElementTypes.EtStruct:
            //                result.Items.Add(GetTreeNode(element));
            //                break;
            //            case Elements.ElementTypes.EtValue:
            //                result.Items.Add(new TreeViewItem() { ToolTip = element.GetValueType().ToString() }.SetField(element.GetDisplayName(), element.GetValue(), 100));
            //                break;
            //            //case Elements.ElementTypes.EtFlag:
            //            //    break;
            //            //case Elements.ElementTypes.EtFlag:
            //            //    break;
            //            //case Elements.ElementTypes.EtStringListTerminator:
            //            //    break;
            //            //case Elements.ElementTypes.EtUnion:
            //            //    break;
            //            //case Elements.ElementTypes.EtStructChapter:
            //            //    break;
            //            default:
            //            result.Items.Add(   new TreeViewItem() { ToolTip = handle.GetElementType().ToString() }.
            //                                SetTextBlockHeader(element.GetDisplayName() + " " + element.GetElementType().ToString()));
            //            break;
            //    }
                    
            //}
            return result;
        }
        public static ComboBox GetComboBox(this PluginRecords<TextureSet> textureSets,string value = "") {
            var cb = new ComboBox();
            int selectedIndex = -1;
            for (int i = 0; i < textureSets.Count; i++)
            {
                var curValue = textureSets[i].ToString();
                cb.Items.Add(curValue);
                if (curValue == value) selectedIndex = i;
            }
            cb.SelectedIndex = selectedIndex;
            return cb;
        }
    }
}
