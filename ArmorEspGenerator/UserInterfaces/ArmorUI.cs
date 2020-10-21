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
        public static TreeViewItem AddWorldModelTreeNode(this MultiSelectTreeView treeview, WorldModel worldModel)
        {
            var result = new TreeViewItem() {
                ItemContainerStyle = (Style)MainWindow.current.FindResource("StretchTreeViewItemStyle"),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            }.SetTextBlockHeader(worldModel.signature);
            treeview.Items.Add(result);
            var modelTreeitem = new TreeViewItem()
            {
                ItemContainerStyle = (Style)MainWindow.current.FindResource("StretchTreeViewItemStyle"),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            result.Items.Add(modelTreeitem);
            var pathSelector = new PathSelector()
            {
                InitialDirectory = Plugin.GetMeshesPath(),
                label = "Model",
                Text = worldModel.Model,
                FileFilter = new FileTypeFilter("Nif File", ".nif").ToString(),
                OnlyAllowValueFromInitialDirectory = true,
                Background = null,
                MinWidth = 10,
                labelWidth = 100,
                ShowInitialDirectoryIfEmpty = true
            };
            pathSelector.GetPathBy += (txt) => txt == "" ? "" : Plugin.GetMeshesPath(txt);
            pathSelector.SetPathBy += (txt) => Plugin.TrimMeshesPath(txt);
            pathSelector.TextChanged += (s, e) => {
                worldModel.Model = pathSelector.Text;
                MainWindow.current.ShowSelectedRecord();
            };
            modelTreeitem.Header = pathSelector;

            var menuitem = pathSelector.labelControl.AddMenuItem("BodyParts to Partitions");
            result.IsExpanded = true;
            menuitem.Click += (s, e) => {
                //worldModel.BodyPartsToPartitions();
                MainWindow.current.ShowSelectedRecord();
            };

            var AlternateTexturesRoot = result.AddTreeNode<TextBlock>("AlternateTextures");
            AlternateTexturesRoot.IsExpanded = true;
            for (int i = 0; i < worldModel.ShapesNames.Count; i++)
            {
                var shapeName = worldModel.ShapesNames[i];
                var data = worldModel.alternateTextures.Find(d => d.ShapeName == shapeName);

                Button btn = new Button()
                {
                    Content = data ? data.NewTexture.ToString() : "    null    ",
                };

                btn.PreviewMouseUp += (s, e) =>
                {
                    ContextMenu contextMenu = new ContextMenu();
                    foreach (var pluginName in Setup.GetLoadedFileNames())
                    {
                        string menuItemName = pluginName;
                        if (pluginName == Plugin.current.GetTempName())
                            menuItemName = Plugin.current.PluginName;
                        var curPluginMenuItem = contextMenu.AddMenuItem(menuItemName);
                        var textureSets = Plugin.GetActivePluginRecords(pluginName, TextureSet.Signature);
                        foreach (var txts in textureSets)
                        {
                            var curTxtsitem = curPluginMenuItem.AddMenuItem(txts.GetEditorID());
                            curTxtsitem.Click += (ss, ee) =>
                            {
                                worldModel.alternateTextures.Set(shapeName, txts);
                                MainWindow.current.ShowSelectedRecord();
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
                var shapeFieldTreeItem = AlternateTexturesRoot.AddTreeNode("", btn);
                var tbk = shapeFieldTreeItem.GetMixControl<TextBlock>(0);
                shapeFieldTreeItem.KeyDown += (s, e) => {
                    if (e.Key == Key.Delete)
                    {
                        worldModel.alternateTextures.Remove(shapeName);
                        btn.Content = "    null    ";
                    }
                };
                tbk.Inlines.Add(new Run(i.ToString() + " . ") { FontWeight = FontWeights.Bold });
                tbk.Inlines.Add(shapeName+" ");
                tbk.IsHitTestVisible = false;
            }
            return result;
        }
        public static string ToLabel(this PartitionFlag[] values,string seperator)
                        => values.Where(d => d.IsEnable).Select(d => d.ToString()).Join(" , ");

        public static TreeViewItem BipedBodyTemplateField(BipedBodyTemplate bipedBodyTemplate)
        {
            var result = new TreeViewItem()
            {
                IsExpanded = true
            }.SetTextBlockHeader("Biped Body Template");

            var partitionsComboBox = new ComboBox();
            var LabelItem = new ComboBoxItem()
            {
                Content = bipedBodyTemplate.FirstPersonFlags.ToLabel(",")
            };
            partitionsComboBox.Items.Add(LabelItem);
            LabelItem.Visibility = Visibility.Collapsed;
            partitionsComboBox.SelectedIndex = 0;
            Action ConfirmModifyParitions = () =>
            {
                var results = bipedBodyTemplate.FirstPersonFlags;
                for (int i = 1; i < partitionsComboBox.Items.Count; i++)
                {
                    results[i - 1].IsEnable = (bool)((ComboBoxItem)partitionsComboBox.Items[i]).GetMixControl<CheckBox>(0).IsChecked;
                }
                bipedBodyTemplate.FirstPersonFlags = results;
                ((ComboBoxItem)partitionsComboBox.Items[0]).Content = results.ToLabel(",");
                partitionsComboBox.SelectedIndex = 0;
            };
            var flags = bipedBodyTemplate.FirstPersonFlags;
            for (int i = 0; i < bipedBodyTemplate.FirstPersonFlags.Length; i++)
            {
                var cb = new CheckBox() { IsChecked = flags[i].IsEnable };
                cb.Checked += (s, e) => { ConfirmModifyParitions(); };
                cb.Unchecked += (s, e) => { ConfirmModifyParitions(); };
                partitionsComboBox.Items.Add(new ComboBoxItem().SetControlLabel(" " + flags[i].ToString(), cb, true));
            }
            partitionsComboBox.PreviewMouseRightButtonDown += (s, e) =>
            {
                for (int i = 1; i < partitionsComboBox.Items.Count; i++)
                    ((ComboBoxItem)partitionsComboBox.Items[i]).GetMixControl<CheckBox>(0).IsChecked = false;
                ConfirmModifyParitions();
            };
            partitionsComboBox.DropDownClosed += (s, e) => partitionsComboBox.SelectedIndex = 0;

   

            result.Items.Add(new TreeViewItem().SetControlLabel(bipedBodyTemplate.FirstPersonFlagsKey + " ", partitionsComboBox));

            //var ArmorTypeComboBox = new ComboBox();
            //ArmorTypeComboBox.Items.Add("Cloth");
            //ArmorTypeComboBox.Items.Add("Heavy");
            //ArmorTypeComboBox.Items.Add("Light");
            //result.AddTreeNode("Armor Type", ArmorTypeComboBox);
            return result;
        }
    }
}
