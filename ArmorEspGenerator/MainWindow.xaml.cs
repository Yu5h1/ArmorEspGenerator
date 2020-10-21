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
using System.Windows.Data;

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
            try
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
                    if (Plugin_cb.SelectedItem != null) {
                        if (File.Exists(Plugin.GetPluginFullPath(Plugin_cb.Text)))
                        {
                            LoadPlugin();
                        }
                    }
                };
                newPlugin_btn.Visibility = Visibility.Hidden;
                this.FocusControlWhenMouseDown(grid);
            }
            catch (Exception error)
            {
                error.Message.PromptError();
                Close();
            }
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (!IsSettingLoaded) {
                try
                {
                    Plugin_cb.Text = settings.LastPlugin;
                    GameMode_cb.Items.Add((Setup.GameMode)3);
                    GameMode_cb.Items.Add((Setup.GameMode)4);
                    GameMode_cb.SelectedItem = Plugin.ParseGameMode(settings.GameMode);
                    CheckNewPluginIsAllow();
                    LoadPlugin(false);
                    IsSettingLoaded = true;
                    PathSelector.AddInvalidCharactersHandler(Plugin_cb.textBox);
                }
                catch (Exception error)
                {
                    error.PromptError();
                    Close();
                }

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
            DataInfos_treeView.Items.Clear();

            if (curSelectedItem.Tag != null)
            {
                switch (curSelectedItem.Tag)
                {
                    case Armor armor:
                        DataInfos_treeView.Items.Add(armor.handle.GetTreeNode(Armatures.Signature));
                        DataInfos_treeView.Items.Add(ArmorUI.BipedBodyTemplateField(armor.bipedBodyTemplate));
                        DataInfos_treeView.AddWorldModelTreeNode(armor.MaleWorldModel);
                        DataInfos_treeView.AddWorldModelTreeNode(armor.FemaleWorldModel);
                        break;
                    case ArmorAddon armorAddon:
                        DataInfos_treeView.Items.Add(ArmorUI.BipedBodyTemplateField(armorAddon.bipedBodyTemplate));
                        DataInfos_treeView.AddWorldModelTreeNode(armorAddon.MaleWorldModel);
                        DataInfos_treeView.AddWorldModelTreeNode(armorAddon.FemaleWorldModel);
                        break;
                    case TextureSet textureSet:
                        for (int i = 0; i < 8; i++)
                        {
                            //var item = new TreeViewItem().SetField(TextureSet.Names[i], textureSet[i], 75, DataInfos_tree.Width);
                            var item = new TreeViewItem() {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                HorizontalAlignment = HorizontalAlignment.Stretch
                            };
                            DataInfos_treeView.Items.Add(item);
                            //item.HorizontalAlignment = HorizontalAlignment.Stretch;
                            var pathSelector = new PathSelector() {
                                OnlyAllowValueFromInitialDirectory = true,
                                InitialDirectory = Plugin.GetTexturesPath(),
                                label = TextureSet.DisplayNames[i],
                                Text = textureSet[i],
                                FileFilter = new FileTypeFilter("Direct Draw Surface", ".dds").ToString(),
                                Background = null,
                                ShowInitialDirectoryIfEmpty = true
                            };
                            item.Height = pathSelector.Height + 1;
                            int permanentIndex = i;
                            pathSelector.GetPathBy += (txt) => txt == "" ? "" : Plugin.GetTexturesPath(txt);
                            pathSelector.SetPathBy += (txt) =>
                            {
                                var result = Plugin.TrimTexturesPath(txt);
                                return result;
                            };
                            pathSelector.TextChanged += (s, e) => {
                                textureSet[permanentIndex] = pathSelector.Text;
                            };
                            item.Header = pathSelector;
                        }
                        break;
                }
            }
        }
        private void RecordsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowSelectedRecord();
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
                    };
                    break;
                case ArmorAddon armorAddon:
                    menuitem = treeItem.AddMenuItem("Generate Armor(s) By Selected ArmorAddons");
                    menuitem.Click += (s, e) => {
                        var selectedArmorAddons = RecordsTreeView.selectedNodes.Where(d => d.Tag.GetType() == typeof(ArmorAddon)).Select(d => (ArmorAddon)d.Tag).ToArray();

                        plugin.AddArmorByArmaturesFromArmorAddon(selectedArmorAddons.Count() > 1 ? "" :
                            selectedArmorAddons[0].EditorID.RemoveSuffixFrom("AA"), selectedArmorAddons );
                    };
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
            menuItem.Click += (s, e) => records.AddNewItem();               
            foreach (var item in records) CreateRecordTreeItem(root, item);

            return root;
        }
        public void ReStartApplication() {
            Closed += (s, e) => {
                ProcessUtil.Launch(System.Reflection.Assembly.GetExecutingAssembly().Location);
            };
            Close();
        }
        void LoadPlugin(bool Prompt = true)
        {
            if (Plugin_cb.Text == "" ) return;
            string fullPluginPath = Plugin.GetPluginFullPath(Plugin_cb.Text);
            if (!File.Exists(fullPluginPath)) {
                if (Prompt)
                {
                    (fullPluginPath + "\ndoes not exists !").PromptWarnning();
                }
                return;
            }

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
                                    ShowSelectedRecord();
                                }
                            }
                        }, ".nif", "folder");

                        var textureSetsNode = GetRecordContainerTreeNode(plugin.TextureSets);
                        textureSetsNode.ToolTip += "Drop .dds files for add new TextureSets";
                        RecordsTreeView.DeleteIgnoreList.Add(textureSetsNode);
                        
                        textureSetsNode.HandleDragDrop(files =>
                        {
                            foreach (var dropfile in files)
                            {
                                if (Path.GetExtension(dropfile) == ".nif")
                                    plugin.AddTextureSetsByNif(dropfile);
                                else
                                    plugin.AddTextureSetsByDifuseAssets(null, dropfile);
                            }
                            
                        }, ".dds","nif");
                        RecordsTreeView.Items.Add(textureSetsNode);
                        save_btn.IsEnabled = true;
                        Plugin_lb.ToolTip = "Dependencies : \n" + plugin.pluginMasters.ToMasterNames().Join("\n   ");
                    }
                }).Start();
            }
        }
        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            using (var p = new WaitCursorProcess()) {
                p.succeed = plugin.Save();
            }
        }

        private void GameMod_lb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                Setup.GetGamePath(SelectedGameMode).ShowInExplorer();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (App.LaunchWithoutWindow == false)
                {
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
            }
            catch (Exception error)
            {
                error.Message.PromptError();                
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
        void CheckNewPluginIsAllow() {
            if (!File.Exists(Plugin.GetPluginFullPath(Plugin_cb.Text)) &&
                            (Plugin_cb.Text.EndsWith(".esp") || Plugin_cb.Text.EndsWith(".esm")))
            {
                newPlugin_btn.Visibility = Visibility.Visible;
            }
            else newPlugin_btn.Visibility = Visibility.Hidden;
        }
        private void Plugin_cb_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNewPluginIsAllow();
        }

        private void NewPlugin_btn_Click(object sender, RoutedEventArgs e)
        {
            Plugin.CreateNewPlugin(Plugin_cb.Text);
        }
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            grid.Focus();
        }
    }
}
