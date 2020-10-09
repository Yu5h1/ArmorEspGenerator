using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using XeLib;
using XeLib.API;
using Yu5h1Tools.WPFExtension;
using Yu5h1Tools.WPFExtension.CustomControls;

namespace TESV_EspEquipmentGenerator
{
    public static class ArmorUI
    {
        public static void ShowArmorAddonInfos(this MultiSelectTreeView treeview, ArmorAddon armorAddon)
        {
            treeview.Items.Add(PartitionsField(armorAddon.FirstPersonFlags));
            treeview.AddworldModelTreeNode(armorAddon.MaleWorldModel);
            treeview.AddworldModelTreeNode(armorAddon.FemaleWorldModel);
        }
        public static TreeViewItem AddworldModelTreeNode(this MultiSelectTreeView treeview, WorldModel worldModel)
        {
            var result = new TreeViewItem() { HorizontalAlignment = HorizontalAlignment.Stretch }.SetTextBlockHeader(worldModel.KEYS.worldModel);
            treeview.Items.Add(result);
            var pathSelector = new PathSelector()
            {
                InitialDirectory = Plugin.GetMeshesPath(),
                label = "Model",
                Text = worldModel.Model,
                FileFilter = new SelectionDialogFilter("Nif File", ".nif").ToString(),
            };
            pathSelector.Width = 350;
            pathSelector.Background = null;
            pathSelector.GetPathBy += (txt) => txt == "" ? "" : Plugin.GetMeshesPath(txt);
            pathSelector.SetPathBy += (txt) => Plugin.TrimMeshesPath(txt);
            pathSelector.TextChanged += (s, e) => {
                worldModel.Model = pathSelector.Text;
                MainWindow.current.ShowSelectedRecord();
            };
            var modelTreeitem = new TreeViewItem()
            {
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                Header = pathSelector,
            };
            result.Items.Add(modelTreeitem);
            var menuitem = pathSelector.labelControl.AddMenuItem("BodyParts to Partitions");
            result.IsExpanded = true;
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

                Button btn = new Button()
                {
                    Content = data ? data.targetTextureSet.ToString() : "    null    ",
                };

                btn.PreviewMouseUp += (s, e) =>
                {
                    ContextMenu contextMenu = new ContextMenu();
                    foreach (var pluginName in Setup.GetLoadedFileNames())
                    {
                        string menuItemName = pluginName;
                        if (pluginName == worldModel.armorAddon.plugin.GetTempName())
                            menuItemName = worldModel.armorAddon.plugin.PluginName;
                        var curPluginMenuItem = contextMenu.AddMenuItem(menuItemName);
                        var textureSets = Plugin.GetActivePluginRecords(pluginName, TextureSet.Signature);
                        foreach (var txts in textureSets)
                        {
                            var curTxtsitem = curPluginMenuItem.AddMenuItem(txts.GetEditorID());
                            curTxtsitem.Click += (ss, ee) =>
                            {
                                worldModel.alternateTextures.Set(shapeName, txts);
                                btn.Content = txts.GetLabel();
                            };
                        }

                    }
                    var curData = worldModel.alternateTextures.Find(d => d.ShapeName == shapeName);
                    if (curData != null)
                    {
                        var deleteMenuitem = contextMenu.AddMenuItem("Delete", "Del");
                        deleteMenuitem.Click += (ds, de) =>
                        {
                            worldModel.alternateTextures.Remove(shapeName);
                            btn.Content = "    null    ";
                        };
                    }
                    contextMenu.IsOpen = true;
                };
                var shapeFieldTreeItem = AlternateTexturesRoot.AddTitleControl("", btn);
                var tbk = shapeFieldTreeItem.GetMixControl<TextBlock>(0);
                shapeFieldTreeItem.KeyDown += (s, e) => {
                    if (e.Key == Key.Delete)
                    {
                        worldModel.alternateTextures.Remove(shapeName);
                        btn.Content = "    null    ";
                    }
                };
                tbk.Inlines.Add(new Run(i.ToString() + " . ") { FontWeight = FontWeights.Bold });
                tbk.Inlines.Add(shapeName);
                tbk.IsHitTestVisible = false;
            }
            return result;
        }
        public static string ToString(this PartitionFlag[] values)
        {
            var valuesArray = values.ToArray();
            var result = "";
            for (int i = 0; i < valuesArray.Length; i++)
                if (valuesArray[i].IsEnable) result += ((int)valuesArray[i].Partition).ToString() + " - " + valuesArray[i].Partition.ToString() + ",";
            return result.RemoveSuffixFrom(",");
        }
        public static TreeViewItem PartitionsField(Handle handle)
        {
            var comboBox = new ComboBox();
            var datas = PartitionsUtil.GetPartitionFlags(handle);
            var LabelItem = new ComboBoxItem() { Content = ToString(datas) };
            comboBox.Items.Add(LabelItem);
            LabelItem.Visibility = Visibility.Collapsed;
            comboBox.SelectedIndex = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                int index = i;
                var partitionFlag = datas[i];
                var cb = new CheckBox() { IsChecked = partitionFlag.IsEnable };
                cb.Checked += (s, e) => datas[index].IsEnable = true;
                cb.Unchecked += (s, e) => datas[index].IsEnable = false;
                comboBox.Items.Add(new ComboBoxItem().SetControlWithLabel(
                    " " + ((int)partitionFlag.Partition) + " - " + partitionFlag.Partition,
                    cb, true));
            }
            Action ConfirmModifyParitions = () => {
                handle.SetValue(PartitionsUtil.ConvertPartitionsToFlagsValue(datas));
                ((ComboBoxItem)comboBox.Items[0]).Content = datas.ToString();
                comboBox.SelectedIndex = 0;
            };
            comboBox.PreviewMouseRightButtonDown += (s, e) => {
                for (int i = 0; i < datas.Length; i++) datas[i].IsEnable = false;
                ConfirmModifyParitions();
            };
            comboBox.DropDownClosed += (s, e) => ConfirmModifyParitions();
            return new TreeViewItem().SetControlWithLabel("First Person Flags ", comboBox);
        }
    }
}
