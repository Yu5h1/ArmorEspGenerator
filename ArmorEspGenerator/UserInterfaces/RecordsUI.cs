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
        public static TreeViewItem GetTreeNode(this Handle handle,string elementName, bool EnabledEdit = false) {
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
            return elementHandle.GetTreeNode(EnabledEdit);
        }
        public static TreeViewItem GetTreeNode(this Handle handle,bool EnabledEdit = false)
        {
            if (handle == null) return null;

            var result = new TreeViewItem() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemContainerStyle = (Style)MainWindow.current.FindResource("StretchTreeViewItemStyle")
            }.SetTextBlockHeader(handle.GetDisplayName());
            result.Tag = handle;
            result.IsExpanded = true;
            var DefineNames = handle.GetDefineNames();
            result.ToolTip = handle.GetElementType().ToString();
            switch (handle.GetElementType())
            {
                case Elements.ElementTypes.EtFile:
                    break;
                case Elements.ElementTypes.EtMainRecord:
                    result.GetHeader<TextBlock>().Text = handle.GetRecordHeaderFormID();
                    foreach (var define in DefineNames)
                    {
                        result.Items.Add(handle.GetTreeNode(define));
                    }
                    break;
                case Elements.ElementTypes.EtGroupRecord:
                    break;
                case Elements.ElementTypes.EtSubRecord:
                    if (DefineNames.Length == 0)
                    {
                        var textbox = new TextBox()
                        {
                            Text = handle.GetValue(),
                            IsEnabled = EnabledEdit
                        };
                        if (EnabledEdit) textbox.TextChanged += (s, e) => handle.SetValue(textbox.Text);
                        result.SetControlLabel(handle.GetDisplayName(), textbox);
                    } else if (DefineNames.Length == 1)
                    {
                        foreach (var element in handle.GetElements())
                            result.Items.Add(element.GetTreeNode());
                    } else
                    {
                        foreach (var define in DefineNames)
                            result.Items.Add(handle.GetTreeNode(define));
                    }
                    break;
                case Elements.ElementTypes.EtSubRecordStruct:
                    break;
                case Elements.ElementTypes.EtSubRecordArray:
                    foreach (var item in handle.GetElements())
                    {
                        result.Items.Add(item.GetTreeNode());
                    }
                    break;
                case Elements.ElementTypes.EtSubRecordUnion:
                    break;
                case Elements.ElementTypes.EtArray:
                    break;
                case Elements.ElementTypes.EtStruct:
                    break;
                case Elements.ElementTypes.EtValue:
                    result.SetControlLabel(handle.GetDisplayName(), new TextBox()
                    {
                        Text = handle.GetValue(),
                        IsEnabled = EnabledEdit
                    });
                    break;
                case Elements.ElementTypes.EtFlag:
                    break;
                case Elements.ElementTypes.EtStringListTerminator:
                    break;
                case Elements.ElementTypes.EtUnion:
                    break;
                case Elements.ElementTypes.EtStructChapter:
                    break;
            }
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
