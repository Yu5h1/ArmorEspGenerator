using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using XeLib.API;
using Yu5h1Tools.WPFExtension;
using Yu5h1Tools.WPFExtension.CustomControls;
using static InformationViewer;

namespace TESV_EspEquipmentGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow current;
        Properties.Settings settings => Properties.Settings.Default;
        public Plugin plugin;
        public bool IsSettingLoaded = false;
        public Setup.GameMode SelectedGameMode => (Setup.GameMode)GameMode_cb.SelectedItem;

        public MainWindow()
        {
            InitializeComponent();
            current = this;
            if (settings.WindowWidth > 300) Width = settings.WindowWidth;
            if (settings.WindowHeight > 100) Height = settings.WindowHeight;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            RecordsTreeView.SelectedItemChanged += RecordsTreeView_SelectedItemChanged;
            Plugin_lb.MouseDown += (s, e) => {
                if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left) {
                    string pluginPath = Plugin.GetPluginFullPath(Plugin_cb.Text);
                    if (File.Exists(pluginPath))
                        ProcessUtil.ShowInExplorer(pluginPath);
                    else if (Directory.Exists(Plugin.GetGameDataPath()))
                        ProcessUtil.ShowInExplorer(Plugin.GetGameDataPath());

                }
            };
            Plugin_cb.DropDownClosed += (s, e) => {
                if (File.Exists(Plugin.GetPluginFullPath(Plugin_cb.Text))) {
                    LoadPlugin();
                }
            };
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (!IsSettingLoaded) {
                GameMode_cb.Items.Add((Setup.GameMode)3);
                GameMode_cb.Items.Add((Setup.GameMode)4);
                Plugin_cb.Text = settings.LastPlugin;
                GameMode_cb.SelectedItem = Plugin.ParseGameMode(settings.GameMode);                
                LoadPlugin();
                IsSettingLoaded = true;
            }
        }
        private void GameMode_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Plugin.SetGameMode(SelectedGameMode);
            Plugin_cb.sourceItems = Directory.GetFiles(Plugin.GetGameDataPath(), "*.esp").Select(d => Path.GetFileName(d)).ToArray();
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
                        DataInfos_tree.ShowArmorAddonInfos(armorAddon);
                        break;
                    case TextureSet textureSet:
                        for (int i = 0; i < 8; i++)
                        {
                            //var item = new TreeViewItem().SetField(TextureSet.Names[i], textureSet[i], 75, DataInfos_tree.Width);
                            var item = new TreeViewItem();
                            item.HorizontalAlignment = HorizontalAlignment.Stretch;
                            var pathSelector = new PathSelector() {
                                label = TextureSet.Names[i],
                                Text = textureSet[i],
                                FileFilter = new SelectionDialogFilter("Direct Draw Surface", ".dds").ToString()
                            };
                            item.Height = pathSelector.Height + 1;
                            pathSelector.InitialDirectory = Plugin.GetTexturesPath();
                            pathSelector.GetPathBy += (txt) => txt == "" ? "" : Plugin.GetTexturesPath(txt);
                            pathSelector.SetPathBy += (txt) => Plugin.TrimTexturesPath(txt);
                            pathSelector.Background = null;

                            pathSelector.Width = 350;
                            int permanentIndex = i;
                            pathSelector.TextChanged += (s, ee) =>
                            {
                                textureSet[permanentIndex] = pathSelector.Text;
                                if (textureSet[permanentIndex] != pathSelector.Text)
                                    pathSelector.Text = textureSet[permanentIndex];
                            };
                            item.Header = pathSelector;
                            DataInfos_tree.Items.Add(item);
                        }
                        break;
                }
            }
        }
        private void RecordsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowSelectedRecord();
        }
        void GenerateArmorsBySelectedArmorAddons(object sender,RoutedEventArgs routedEventHandler)
        {

        }
        void GenerateArmorBySelectedArmorAddons(object sender, RoutedEventArgs routedEventHandler)
        {

        }
        public void CreateRecordTreeItem<T>(TreeViewItem root,T item) where T : RecordElement<T>
        {
            var treeItem = root.AddNameField(   item.EditorID,false,
                                                (tb)=> {
                                                    if (item.EditorID != tb.Text) item.EditorID = tb.Text;
                                                },
                                                RecordsTreeView.Width-6);

            treeItem.Tag = item;
            treeItem.ToolTip = item.FormID;
            treeItem.Unloaded += (s, e) =>
            {
                var itemTag = ((TreeViewItem)s).Tag;
                if (itemTag != null)
                {
                    T curData = (T)itemTag;
                    curData.Delete();
                }
            };
            MenuItem menuitem = treeItem.AddMenuItem("Duplicate","Ctrl + D");
            
            menuitem.Click += (s, e) => DuplicateSelected();

            switch (item)
            {
                case Armor armor:
                    
                    menuitem = treeItem.AddMenuItem("Generate Armors By Similar Diffuses");
                    menuitem.Click += (s, e) =>
                    {
                        armor.GenerateArmorBySimilarDiffuses("QQ");
                    };
                    break;
                case ArmorAddon armorAddon:
                    menuitem = treeItem.AddMenuItem("Generate Armor By Selected ArmorAddons");
                    menuitem.Click += GenerateArmorBySelectedArmorAddons;
                    menuitem = treeItem.AddMenuItem("Generate Armors By Selected ArmorAddons");
                    menuitem.Click += GenerateArmorsBySelectedArmorAddons;
                    break;
                case TextureSet textureSet:
                    treeItem.HandleDragDrop(files =>
                    {
                        plugin.AddTextureSetsByDifuseAssets(textureSet.ToArray(), files);
                    }, ".dds");
                    break;
            }
        }
        public TreeViewItem GetRecordContainerTreeNode<T>(PluginRecords<T> records) where T : RecordElement<T>
        {
            var root = new TreeViewItem().SetTextBlockHeader(typeof(T).Name + "s");
            records.Added += item => CreateRecordTreeItem(root, item);
            var menuItem = root.AddMenuItem("New");
            menuItem.Click += (s, e) => {
                records.AddNewItem(("New"+typeof(T).Name).GetUniqueStringWithSuffixNumber(records,d=>d.EditorID));
            };
            foreach (var item in records) CreateRecordTreeItem(root, item);

            return root;
        }
        public void ReStartApplication() {
            Closed += (s, e) => {
                ProcessUtil.Launch(System.Reflection.Assembly.GetExecutingAssembly().Location);
            };
            Close();
        }
        void LoadPlugin()
        {
            if (Plugin_cb.Text == "" ) return;

            if (!IsFileExistElsePrompt(Plugin.GetPluginFullPath(Plugin_cb.Text))) return;
            if (plugin) {
                ReStartApplication();
            }else
            {
                loading_lb.Visibility = Visibility.Visible;
                new Timer(0.1, () =>
                {
                    plugin = Plugin.Load(SelectedGameMode, Plugin_cb.Text, true);
                    loading_lb.Visibility = Visibility.Hidden;
                    if (plugin)
                    {
                        var armorsNode = GetRecordContainerTreeNode(plugin.Armors);
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
                        var ArmorAddonsNode = GetRecordContainerTreeNode(plugin.ArmorAddons);
                        RecordsTreeView.DeleteIgnoreList.Add(ArmorAddonsNode);
                        RecordsTreeView.Items.Add(ArmorAddonsNode);
                        ArmorAddonsNode.HandleDragDrop(files =>
                        {
                            foreach (var path in files)
                            {
                                var pathInfo = new PathInfo(path);
                                if (pathInfo.IsDirectory)
                                {

                                }
                                else
                                {
                                    plugin.AddArmorAddonByNifFile(path);
                                }
                            }
                        }, ".nif", "folder");

                        var textureSetsNode = GetRecordContainerTreeNode(plugin.TextureSets);
                        textureSetsNode.ToolTip += "Drop .dds files for add new TextureSets";
                        RecordsTreeView.DeleteIgnoreList.Add(textureSetsNode);
                        textureSetsNode.HandleDragDrop(files =>
                        {
                            plugin.AddTextureSetsByDifuseAssets(null, files);
                        }, ".dds");
                        RecordsTreeView.Items.Add(textureSetsNode);
                        save_btn.IsEnabled = true;
                    }
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
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                Setup.GetGamePath(SelectedGameMode).ShowInExplorer();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (App.LaunchWithoutWindow == false) {
                try
                {
                    plugin?.UnLoad();
                }
                catch (Exception error)
                {
                    error.Message.PromptWarnning();
                    throw;
                }
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;
                settings.LastPlugin = Plugin_cb.Text;
                if (GameMode_cb.SelectedItem != null)
                    settings.GameMode = GameMode_cb.SelectedItem.ToString();
                settings.Save();
            }
            base.OnClosing(e);
        }
        void DuplicateSelected()
        {
            foreach (var item in RecordsTreeView.selectedNodes)
            {
                var curObj = item.Tag as IRecordElement;
                if (curObj != null)
                {
                    curObj.Duplicate();
                }
            }
        }
        private void RecordsTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (RecordsTreeView.selectedNodes.Count > 0) {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.D)
                {
                    DuplicateSelected();
                }
            }

        }

        private void Plugin_cb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                LoadPlugin();
            }
            if (e.Key == Key.Escape) {
                Plugin_cb.Text = "";
            }
        }
    }
}
