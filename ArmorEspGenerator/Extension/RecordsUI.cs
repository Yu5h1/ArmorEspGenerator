﻿using System;
using System.IO;
using System.Windows.Controls;
using XeLib;
using XeLib.API;
using Yu5h1Tools.WPFExtension;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;

namespace TESV_EspEquipmentGenerator
{
    public static class RecordsUI
    {
        public static TreeViewItem GetTreeNode(this WorldModel worldModel)
        {
            var result = new TreeViewItem().SetTextBlockHeader(worldModel.KEYS.worldModel);
            var modelField = result.AddField("Model", worldModel.Model);
            var menuitem = modelField.AddMenuItem("BodyParts to Partitions");
            menuitem.Click += (s, e) => {
                worldModel.BodyPartsToPartitions();
                MainWindow.current.ShowSelectedRecord();
            };

            var AlternateTexturesRoot = result.AddTextBlock("AlternateTextures");
            AlternateTexturesRoot.IsExpanded = true;
            for (int i = 0; i < worldModel.ShapesNames.Count; i++)
            {
                var shapeName = worldModel.ShapesNames[i];
                var data = worldModel.alternateTextures.Find(d => d.ShapeName == shapeName);

                Button btn = new Button() {
                    Content = data ? data.targetTextureSet.ToString() : "    null    ",
                };

                btn.PreviewMouseUp += (s, e) =>
                {
                    ContextMenu contextMenu = new ContextMenu();
                    foreach (var pluginName in Setup.GetLoadedFileNames())
                    {
                        var curPluginMenuItem = contextMenu.AddMenuItem(pluginName);
                        var textureSets = Handle.BaseHandle.GetElement(pluginName).GetElements("Texture Set");
                        foreach (var txts in textureSets)
                        {
                            var curTxtsitem = curPluginMenuItem.AddMenuItem(txts.GetEditorID());
                            curTxtsitem.Click += (txtsS, txtsE) =>
                            {
                                worldModel.alternateTextures.Set(shapeName, txts);
                                btn.Content = txts.GetLabel();
                            };
                        }

                    }
                    var curData = worldModel.alternateTextures.Find(d => d.ShapeName == shapeName);
                    if (curData != null)
                    {
                        var deleteMenuitem = contextMenu.AddMenuItem("Delete");
                        deleteMenuitem.Click += (ds, de) =>
                        {
                            worldModel.alternateTextures.Remove(shapeName);
                            btn.Content = "    null    ";
                        };
                    }
                    contextMenu.IsOpen = true;
                };
                var item = AlternateTexturesRoot.AddTitleControl("", btn);
                var tbk = item.GetMixControl<TextBlock>(0);
                tbk.Inlines.Add(new Run(i.ToString()+" . ") { FontWeight = FontWeights.Bold });
                tbk.Inlines.Add(shapeName);
                tbk.IsHitTestVisible = false;
            }
            
            return result;
        }
        public static TreeViewItem GetPartitionsFieldTreeItem(Handle handle)
        {
            var comboBox = new ComboBox();
            var datas = PartitionsUtil.GetPartitionFlags(handle);
            var displayItem = new ComboBoxItem() { Content = Armor.GetDisplayPartitionFlags(datas) };

            comboBox.Items.Add(displayItem);
            displayItem.Visibility = Visibility.Collapsed;
            comboBox.SelectedIndex = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                int index = i;
                var partitionFlag = datas[i];
                var cb = new CheckBox() { IsChecked = partitionFlag.flag };
                cb.Checked += (s, e) => datas[index].flag = true;
                cb.Unchecked += (s, e) => datas[index].flag = false;
                comboBox.Items.Add(new ComboBoxItem().SetControlWithLabel(
                    " " + ((int)partitionFlag.partition) + " - " + partitionFlag.partition,
                    cb, true));
            }
            Action ConfirmModifyParitions = () => {
                handle.SetValue(PartitionsUtil.ConvertPartitionsToFlagsValue(datas));
                ((ComboBoxItem)comboBox.Items[0]).Content = Armor.GetDisplayPartitionFlags(datas);
                comboBox.SelectedIndex = 0;
            };
            comboBox.PreviewMouseRightButtonDown += (s, e) => {
                for (int i = 0; i < datas.Length; i++) datas[i].flag = false;
                ConfirmModifyParitions();
            };
            comboBox.DropDownClosed += (s, e) => ConfirmModifyParitions();
            return new TreeViewItem().SetControlWithLabel(handle.GetDisplayName(), comboBox);
        }
        public static TreeViewItem GetTreeNode(this Handle handle)
        {
            var result = new TreeViewItem().SetTextBlockHeader(handle.GetDisplayName());
            result.Tag = handle;
            result.ToolTip = handle.GetElementType().ToString();
            result.IsExpanded = true;
            var elements = handle.GetElements();            
            if (elements.Length > 0)
            foreach (var element in elements)
            {
                var DisplayName = element.GetDisplayName();
                switch (element.GetElementType())
                {
                        //case Elements.ElementTypes.EtFile:
                        //    break;
                        case Elements.ElementTypes.EtMainRecord:
                            //result.Items.Add(GetTreeNode(item));
                            break;
                        //case Elements.ElementTypes.EtGroupRecord:
                        //    break;
                        case Elements.ElementTypes.EtSubRecord:
                            result.Items.Add(GetTreeNode(element));
                            break;
                        case Elements.ElementTypes.EtSubRecordStruct:
                            result.Items.Add(GetTreeNode(element));
                            break;
                        case Elements.ElementTypes.EtSubRecordArray:
                            result.Items.Add(GetTreeNode(element));
                            break;
                        //case Elements.ElementTypes.EtSubRecordUnion:
                        //    break;
                        //case Elements.ElementTypes.EtArray:
                        //    break;
                        case Elements.ElementTypes.EtStruct:
                            result.Items.Add(GetTreeNode(element));
                            break;
                        case Elements.ElementTypes.EtValue:
                            if (DisplayName == "First Person Flags") {
                                result.Items.Add(GetPartitionsFieldTreeItem(element));
                            }else
                                result.Items.Add(new TreeViewItem() { ToolTip = element.GetValueType().ToString() }.SetField(element.GetDisplayName(), element.GetValue(), 100));
                            break;
                        //case Elements.ElementTypes.EtFlag:
                        //    break;
                        //case Elements.ElementTypes.EtFlag:
                        //    break;
                        //case Elements.ElementTypes.EtStringListTerminator:
                        //    break;
                        //case Elements.ElementTypes.EtUnion:
                        //    break;
                        //case Elements.ElementTypes.EtStructChapter:
                        //    break;
                        default:
                        result.Items.Add(   new TreeViewItem() { ToolTip = handle.GetElementType().ToString() }.
                                            SetTextBlockHeader(element.GetDisplayName() + " " + element.GetElementType().ToString()));
                        break;
                }
                    
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