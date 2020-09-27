using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using XeLib;
using XeLib.API;
using System.Windows.Input;
using static InformationViewer;
using System.Windows.Controls;
using Yu5h1Tools.WPFExtension;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TESV_EspEquipmentGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow current;
        Properties.Settings settings => Properties.Settings.Default;
        Setup.GameMode curremtGameMode {
            get {
                if (settings.GameMode == "") 
                    settings.GameMode = Setup.GameMode.SSE.ToString();
                return (Setup.GameMode)Enum.Parse(typeof(Setup.GameMode), settings.GameMode);
            }
            set {
                Plugin.CurrentGameMode = value;
                settings.GameMode = value.ToString();
            } 
        }
        public Plugin plugin;
        public bool IsSettingLoaded = false;
        public MainWindow()
        {
            InitializeComponent();
            current = this;
            if (settings.WindowWidth > 300)
                Width = settings.WindowWidth;
            if (settings.WindowHeight > 100)
                Height = settings.WindowHeight;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            RecordsTreeView.SelectedItemChanged += RecordsTreeView_SelectedItemChanged;
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (!IsSettingLoaded) {

                GameMode_cb.Items.Add((Setup.GameMode)3);
                GameMode_cb.Items.Add((Setup.GameMode)4);
                Plugin_cb.Text = settings.LastPlugin;
                GameMode_cb.SelectedItem = curremtGameMode;
                
                IsSettingLoaded = true;
            }
        }
        private void GameMode_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curremtGameMode = (Setup.GameMode)GameMode_cb.SelectedItem;
            Plugin_cb.sourceItems = Directory.GetFiles(Plugin.GetGameDataPath(), "*.esp").Select(d => Path.GetFileName(d)).ToArray();
            Load_btn.IsEnabled = File.Exists(Plugin.GetPluginFullPath(Plugin_cb.Text));
        }
        public void ShowSelectedRecord()
        {
            var curSelectedItem = RecordsTreeView.selectedNode;
            if (curSelectedItem == null) return;
            DataInfos_tree.Items.Clear();
            if (curSelectedItem.Tag != null)
            {
                switch (curSelectedItem.Tag)
                {
                    case Armor armor:
                        DataInfos_tree.Items.Add(armor.handle.GetTreeNode());
                        break;
                    case ArmorAddon armorAddon:
                        DataInfos_tree.Items.Add(RecordsUI.GetPartitionsFieldTreeItem(armorAddon.FirstPersonFlags));
                        DataInfos_tree.Items.Add(armorAddon.MaleWorldModel.GetTreeNode());
                        DataInfos_tree.Items.Add(armorAddon.FemaleWorldModel.GetTreeNode());
                        break;
                    case TextureSet textureSet:
                        for (int i = 0; i < 8; i++)
                        {

                            var item = new TreeViewItem().SetField(TextureSet.Names[i], textureSet[i], 75, DataInfos_tree.Width);
                            DataInfos_tree.Items.Add(item);
                            int permanentIndex = i;
                            var tb = item.GetMixControl<TextBox>(1);
                            item.GetMixControl<TextBlock>(0).MouseUp += (tbk, tbke) => {
                                if (tbke.ChangedButton == MouseButton.Right)
                                {
                                    ProcessUtil.ShowInExplorer(Plugin.GetTexturesPath(tb.Text));
                                }
                            };
                            tb.TextChanged += (s, ee) =>
                            {
                                textureSet[permanentIndex] = ((TextBox)s).Text;
                            };
                            tb.HandleDragDrop(files => {
                                if (files.Length == 1)
                                {
                                    if (Plugin.IsLocateAtGameAssetsFolder(files[0]))
                                    {
                                        tb.Text = textureSet[permanentIndex] = Plugin.TrimTexturesPath(files[0]);
                                    }
                                }
                            }, ".dds");
                        }
                        break;
                }
            }
        }
        private void RecordsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowSelectedRecord();
        }
        public void CreateTreeItem<T>(TreeViewItem root,T item) where T : RecordElement<T>
        {
            var treeItem = root.AddNameField(item.EditorID,RecordsTreeView.Width-6);
            treeItem.Tag = item;
            //treeItem.ToolTip = item.FormID;
            treeItem.Unloaded += (s, e) =>
            {
                var itemTag = ((TreeViewItem)s).Tag;
                if (itemTag != null)
                {
                    T curData = (T)itemTag;
                    curData.Delete();
                }
            };
            switch (item)
            {
                case Armor armor:
                    var menuitem = treeItem.AddMenuItem("Generate Armors By Similar Diffuses");
                    menuitem.Click += (s, e) =>
                    {
                        armor.GenerateArmorBySimilarDiffuses("XF_Bella_Bra");
                    };

                    break;
                case TextureSet textureSet:
                    treeItem.ToolTip += "Drop .dds files for duplicated TextureSets from it";
                    treeItem.HandleDragDrop(files =>
                    {
                        plugin.AddTextureSetsByDifuseAssets(textureSet.ToArray(), files);
                    }, ".dds");
                    break;
            }
        }
        public TreeViewItem GetTreeNode<T>(PluginRecords<T> records) where T : RecordElement<T>
        {
            var root = new TreeViewItem().SetTextBlockHeader(typeof(T).Name + "s");
            records.Added += item => CreateTreeItem(root, item);
            foreach (var item in records)
                CreateTreeItem(root, item);
            return root;
        }
        private void Load_btn_Click(object sender, RoutedEventArgs e) => LoadPlugin();

        void LoadPlugin()
        {
            plugin?.UnLoad();
            if (File.Exists(Plugin.GetPluginFullPath(Plugin_cb.Text)))
            {
                loading_lb.Visibility = Visibility.Visible;
                //loading_lb.Refresh();
                new Timer(0.1, () =>
                {
                    //Plugin.printLoadingLog = true;
                    plugin = Plugin.Load(Plugin.CurrentGameMode, Plugin_cb.Text, true);
                    loading_lb.Visibility = Visibility.Hidden;
                    if (plugin)
                    {
                        var armorsNode = GetTreeNode(plugin.Armors);
                        RecordsTreeView.DeleteIgnoreList.Add(armorsNode);
                        RecordsTreeView.Items.Add(armorsNode);
                        armorsNode.HandleDragDrop(files =>
                        {
                            foreach (var path in files)
                            {
                                var pathInfo = new PathInfo(path);
                                if (pathInfo.IsDirectory)
                                {

                                }
                                else
                                {
                                    plugin.GenerateArmorByNifFile(path);
                                }
                            }
                        }, ".nif", "folder");


                        var ArmorAddonsNode = GetTreeNode(plugin.ArmorAddons);
                        RecordsTreeView.DeleteIgnoreList.Add(ArmorAddonsNode);
                        RecordsTreeView.Items.Add(ArmorAddonsNode);

                        var textureSetsNode = GetTreeNode(plugin.TextureSets);
                        textureSetsNode.ToolTip += "Drop .dds files for add new TextureSets";
                        RecordsTreeView.DeleteIgnoreList.Add(textureSetsNode);
                        textureSetsNode.HandleDragDrop(files =>
                        {
                            plugin.AddTextureSetsByDifuseAssets(null, files);
                        }, ".dds");
                        RecordsTreeView.Items.Add(textureSetsNode);                        
                    }
                    save_btn.IsEnabled = true;
                }).Start();
            }
            
        }
        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            using (var p = new WaitCursorProcess()) {
                if (plugin.Save()) p.PlayCompletedSound = true;
            }
        }

        private void GameMod_lb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                Setup.GetGamePath(curremtGameMode).ShowInExplorer();
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (App.LaunchWithoutWindow == false) {
                plugin?.UnLoad();
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;
                settings.LastPlugin = Plugin_cb.Text;
                if (GameMode_cb.SelectedItem != null)
                    settings.GameMode = GameMode_cb.SelectedItem.ToString();
                settings.Save();
            }
            base.OnClosing(e);
        }

        private void Plugin_cb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Load_btn.IsEnabled = File.Exists(Plugin.GetPluginFullPath(Plugin_cb.Text));
        }
    }
}
