using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using XeLib.API;
using XeLib;
using static InformationViewer;
using Yu5h1Tools.WPFExtension;
using System.Threading.Tasks;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace TESV_EspEquipmentGenerator
{
    public partial class Plugin : RecordElement
    {
        public static Handle BaseHandle
        {
            get {
                return Handle.BaseHandle;
            } 
        }
        public static Dictionary<string, string> GetGlobalInfos()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            string content = Meta.GetGlobals();
            if (content != "")
            {
                foreach (var line in content.GetLines())
                {
                    if (line.Contains('='))
                    {
                        var data = line.Split('=');
                        results.Add(data[0], data[1]);
                    }
                }
            }
            return results;
        }
        public static bool IsGameSpecifed => GetGlobalInfos().ContainsKey("GameName");

        public static Plugin current;
        public List<Handle> defaultRaces => SkyrimESM.GetRecords("RACE",false).ToList();

        public static string GetLoadedGameName()
        {
            try { return Meta.GetGlobal("AppName"); } catch (Exception) { return ""; }
        }
        public static bool IsLoaded => Setup.GetLoadedFileNames().Length > 0;

        public static Setup.GameMode ParseGameMode(string modeName) {
            try
            {
                 return (Setup.GameMode)Enum.Parse(typeof(Setup.GameMode), modeName);
            }
            catch (Exception e)
            {
                e.Message.print();
                return Setup.GameMode.SSE;
            }
        }

        public override string signature => "TES4";

        public string PluginName;
        public string FullPath => GetPluginFullPath(PluginName);
        public string GetTempName() => PluginName.ToLower().Replace(".esp", "_temp.esp");
        public string GetTempFullPath() => GetPluginFullPath(GetTempName());
        public static string GetTempName(string name) => name.ToLower().Replace(".esp", "_temp.esp");
        public static string GetTempFullPath(string name) => GetPluginFullPath(GetTempName(name));

        public FileHeader fileHeader;        

        void Init(string pluginName) {
            PluginName = pluginName;
            print("Construct " + pluginName + " {Plugin} instance.");

            TextureSets = new PluginRecords<TextureSet>(this, TextureSet.Create);
            ArmorAddons = new PluginRecords<ArmorAddon>(this, ArmorAddon.Create);
            Armors = new PluginRecords<Armor>(this, Armor.Create);
            fileHeader = new FileHeader(this);

            print(pluginName + " is generated ! ");
        }

        public Plugin(string pluginName, params string[] masterfiles) : base(BaseHandle, BaseHandle.AddElement(pluginName))
        {
            Init(pluginName);
            fileHeader.masters.Add(masterfiles);
        }

        Plugin(Handle target, string pluginName) : base(Handle.BaseHandle,target) {
            Init(pluginName);
        }
        public string Author { get => GetValue(@"File Header\CNAM"); set => SetValue(@"File Header\CNAM", value); }
        public string Description { get => GetValue(@"File Header\SNAM"); set => SetValue(@"File Header\SNAM", value); }

        public Handle[] GetRecords(string recordSignature = "", bool includeOverrides = false) => handle.GetRecords(recordSignature, includeOverrides);

        public PluginRecords<TextureSet> TextureSets;
        public PluginRecords<Armor> Armors;
        public PluginRecords<ArmorAddon> ArmorAddons;


        public bool Save() {
            if (IsFileNotLockedElsePopup(FullPath))
            {
                using (var p = new WaitCursorProcess())
                {
                    Files.SaveFile(handle, FullPath);
                }
                return true;
            }
            return false;
        }
        public void UnLoad() {
            if (File.Exists(GetTempFullPath())) {
                //Setup.UnloadPlugin(handle);
                Meta.Close();
                File.Delete(GetTempFullPath());
            }
        }
        static string SpecialGamePathCache = "";
        public static string SpecialGamePath {
            get { return SpecialGamePathCache; }
            set {
                Setup.SetGamePath(value);
                SpecialGamePathCache = value;
            }
        }
        public static Setup.GameMode currentGameMode = Setup.GameMode.SSE;
        public static string GetGamePath() => Setup.GetGamePath(currentGameMode);
        public static String GetGameDataPath(Setup.GameMode gameMode) => Path.Combine(Setup.GetGamePath(gameMode), "Data");
        public static String GetGameDataPath() => Path.Combine(GetGamePath(), "Data");
        public static String GetGameDataPath(string suffix) => GetGameDataPath().CombineNoleadSlash(suffix).ReplaceRepeatFolderUtilEmpty("Data");

        public static String GetMeshesPath() => Path.Combine(GetGameDataPath(), "Meshes");
        public static String GetMeshesPath(string suffix) =>
                                GetMeshesPath().CombineNoleadSlash(suffix).ReplaceRepeatFolderUtilEmpty("Meshes");

        public static String GetTexturesPath() => Path.Combine(GetGameDataPath(), "Textures");
        public static String GetTexturesPath(string suffix) =>
                                GetTexturesPath().CombineNoleadSlash(suffix).ReplaceRepeatFolderUtilEmpty("Textures");
        public static string GetPluginFullPath(string fileName) => Path.Combine(GetGameDataPath(), fileName);

        public static bool IsLocateAtGameAssetsFolder(string path)
        {
            if (!ContainDataFolderInPath(path)) return true;
            var result = path.ToLower().Contains(GetGameDataPath().ToLower());
            if (!result) (GetGameDataPath()+"\n\n"+path+"\n\nThe Drop files are not locate at Selected Game's folder").PopupWarnning();
            return result;
        }

        public static bool ContainDataFolderInPath(string path) => path.ToLower().Contains(@"\data\");
        public static bool ContainTexturesFolderInPath(string path) => path.ToLower().Contains(@"\textures\");
        public static bool ContainMeshesFolderInPath(string path) => path.ToLower().Contains(@"\meshes\");
        public static  string[] GetMasterNames(string PluginName) {
            var results = new string[0];
            if (File.Exists(GetPluginFullPath(PluginName))) {
                try
                {
                    var header = Setup.LoadPluginHeader(PluginName);
                    if (header == null)
                    {
                        (PluginName + " has no header.").PopupWarnning();
                    } else
                    {
                        var masterfiles = header.GetElements()[0].GetElement("Master Files");
                        if (masterfiles != null) {
                            results = masterfiles.GetElements().Select(d=>d.GetValue("MAST")).ToArray();
                            Setup.UnloadPlugin(header);
                        }
                    }
                }
                catch (Exception error)
                {
                    error.Message.PopupError();
                }
            }
            return results;
        }
        
        public static string[] GetMissingMastersInfo(params string[] PluginsList) {
            List<string> missingMasters = new List<string>();

            try
            {
                foreach (var pluginName in PluginsList)
                {
                    if (pluginName.Equals("Skyrim.esm",StringComparison.OrdinalIgnoreCase)) continue;
                    if (File.Exists(GetPluginFullPath(pluginName)))
                    {
                        var mastersName = GetMasterNames(pluginName);
                        foreach (var master in mastersName)
                        {
                            if (!File.Exists(GetPluginFullPath(master)))
                            {
                                if (!missingMasters.Contains(master))
                                    missingMasters.Add(master);
                            }
                        }
                    } else if (!missingMasters.Contains(pluginName))
                        missingMasters.Add(pluginName);
                }
            } catch (Exception e)
            {
                e.Message.PopupError();
                throw;
            }

            return missingMasters.ToArray();
        }
        public static Handle GetActivePluginHandle(string pluginName)
                                                    => Handle.BaseHandle.GetElement(pluginName);
        public static Handle[] GetActivePluginRecords(string pluginName,string search = "",bool includeOverrides = false)
                                            => GetActivePluginHandle(pluginName).GetRecords(search,includeOverrides);
        public static Handle FindRecord(Predicate<Handle> predicate,string signature, bool includeOverrides = false)
        {
            var result = current.handle.FindRecord(predicate,signature, includeOverrides);
            if (result == null) {
                var masters = current.fileHeader.masters;
                var found =GetActivePluginHandle(masters.GetMasterName(0)).FindRecord(d=>d.GetEditorId()== "ArmorHeavy","KYWD",true);

                for (int i = masters.Count - 1; i >= 0; i--)
                {
                    result = GetActivePluginHandle(masters.GetMasterName(i)).FindRecord(predicate,signature,includeOverrides);
                    if (result != null) break;
                }
            }
            return result;
        }
        public static bool SetGameMode(Setup.GameMode gameMode) {
            
            try
            {
                currentGameMode = gameMode;
                print("Initializing XEditLib");
                Meta.Initialize();
                print("Setting game mode to "+ gameMode.ToString());
                Meta.Initialize();
                Setup.SetGameMode(gameMode);
            }
            catch (Exception error)
            {
                (error.Message+"\nRestart program.").print();
                MainWindow.current.ReStartApplication();
            }
            return true;
        }
        public static bool LoadPlugins(params string[] PluginsList)
        {
            var missingMasters = GetMissingMastersInfo(PluginsList);
            if (missingMasters.Length > 0)
            {
                ("The dependent modules are missing : \n" + missingMasters.Join("\n")).PopupWarnning();
                return false;
            }
            try
            {
                Setup.LoadPlugins(string.Join("\n", PluginsList));
                print("Loading...");
            }
            catch (Exception error)
            {
                error.Message.PopupWarnning();
                return false;
            }
            var state = Setup.LoaderState.IsInactive;
            while (state != Setup.LoaderState.IsDone && state != Setup.LoaderState.HasError) state = Setup.GetLoaderStatus();
            Console.WriteLine(Messages.GetMessages());
            Messages.ClearMessages();

            return true;
        }
        public static Plugin Load(string pluginName, bool AsTemp = false)
        {
            if (!IsGameSpecifed)
            {
                "No Game type specified.".PopupWarnning();
                return null;
            }
            string espName = pluginName;
            var fullPluginPath = GetPluginFullPath(pluginName);
            if (!File.Exists(fullPluginPath)) return null;
            if (AsTemp)
            {
                espName = GetTempName(pluginName);
                var tempPath = GetPluginFullPath(espName);
                if (IsFileNotLockedElsePopup(tempPath))
                    File.Copy(fullPluginPath, tempPath, true);
            }
            if (LoadPlugins(espName))
            {
                current = new Plugin(Elements.GetElement(Handle.BaseHandle, espName), pluginName);
                return current;
            } else return null;
        }

        public static void CreateNewPlugin( Setup.GameMode gameMod,string pluginName,
                                            Func<Plugin,NotifyIcon,bool> workflow,
                                            params string[] masterfiles) {
            Meta.Initialize();
            currentGameMode = gameMod;
            Setup.SetGameMode(gameMod);            
            var fullPathInfo = new PathInfo(Setup.GetGamePath(gameMod) + @"\Data\" + pluginName);
            if (IsFileLockedPopup(fullPathInfo)) {
                App.Current.Shutdown();
                return;
            }

            if (File.Exists(fullPathInfo))
            {
                if ((fullPathInfo.fullPath + " already exists. Do you want to overwrite it ? ").
                     YesNoBox("Plugin Overwrite Query")) File.Delete(fullPathInfo);
                else { 
                    App.Current.Shutdown();
                    return;
                }
            }

            var title = "Generating " + gameMod.ToString() + @"...data\" + pluginName;

            var notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            notifyIcon.SetProcessIcon(0);
            notifyIcon.ShowBalloonTip(1000, " Starting Generate Esp", title, System.Windows.Forms.ToolTipIcon.Info);
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add("Closed", (s, e) => {
                notifyIcon.Icon = null;
                App.Current.Shutdown();
            });

            Task.Run(new Func<Handle>(() =>
            {
                List<string> masterfileslist = new List<string>() { "Skyrim.esm" };
                masterfileslist.AddRange(masterfiles);

                notifyIcon.Text = "10%";
                notifyIcon.SetProcessIcon(0.1);
                Setup.LoadPlugins(masterfileslist.Join("\n"));
                var state = Setup.LoaderState.IsInactive;
                while (state != Setup.LoaderState.IsDone && state != Setup.LoaderState.HasError)
                {
                    state = Setup.GetLoaderStatus();
                }
                Messages.ClearMessages();

                var pluginFileHandle = Files.AddFile(pluginName);
                current = new Plugin(pluginName, masterfileslist.ToArray());
                workflow?.Invoke(current, notifyIcon);
                return pluginFileHandle;
            })).ContinueWith(task=> {
                notifyIcon.SetProcessIcon(1);
                Files.SaveFile(task.Result, fullPathInfo);
                //active plugin
                var activationPathInfo = new PathInfo(Path.Combine(Meta.GetGlobal("AppDataPath"), "plugins.txt"));
                if (activationPathInfo.Exists && !activationPathInfo.IsLocked)
                {
                    var activationInfo = File.ReadAllLines(activationPathInfo).ToList();

                    var PluginIndex = activationInfo.FindIndex(d => d.EndsWith(pluginName, StringComparison.OrdinalIgnoreCase));

                    string pluginActivateInfo = pluginName;
                    if (gameMod == Setup.GameMode.SSE) pluginActivateInfo = "*" + pluginActivateInfo;

                    if (PluginIndex < 0) activationInfo.Add(pluginActivateInfo);
                    else activationInfo[PluginIndex] = pluginActivateInfo;

                    File.WriteAllLines(activationPathInfo, activationInfo);
                }
                notifyIcon.ShowBalloonTip(100, " Completed ! ", title+" is Finished.", System.Windows.Forms.ToolTipIcon.Info);
                notifyIcon.BalloonTipClosed += (s, e) => {
                    notifyIcon.Icon = null;
                    App.Current.Shutdown();
                };
            });
        }
    }
    public class FileHeader : RecordObject
    {
        public Plugin plugin;
        public static string Signature => "File Header";
        public override string signature => Signature;
        public string Author {
            get => handle.GetValue("CNAM");
            set => handle.SetValue("CNAM",value);
        }
        public string Description
        {
            get => handle.GetValue("SNAM");
            set => handle.SetValue("SNAM", value);
        }
        public PluginMasters masters;
        public FileHeader(Plugin plugin) : base(plugin.handle) {
            this.plugin = plugin;
            masters = new PluginMasters(this);
        }

    }
    public class PluginMasters : RecordArrays<Handle>
    {
        public static string Signature => "Master Files";
        public override string signature => Signature;

        public FileHeader fileHeader;
        public PluginMasters(FileHeader fileHeader) : base(fileHeader.handle) {
            this.fileHeader = fileHeader;
            if (fileHeader.handle == null) "FileHeader is null".PopupWarnning();
            else if (handle == null)
                Masters.AddMaster(fileHeader.plugin.handle, "Skyrim.esm");
            else AddRange(handle.GetElements());
            
        }
        public string GetMasterName(int index) => this[index].GetValue("MAST");
        public void Add(params string[] masterFiles)
        {            
            if (masterFiles.Length > 0) {
                Clear();
                foreach (var master in masterFiles)
                {
                    if (!Exists(master))
                    {
                        Masters.AddMaster(fileHeader.plugin.handle, master);
                    }
                }
                AddRange(handle.GetElements());
            }
        }
        public void Remove(string masterFile)
        {
            var item = Find(d => d.GetValue("MAST").Equals(masterFile, StringComparison.OrdinalIgnoreCase));
            if (item != null) item.Delete();
            
        }
        public bool Exists(string masterfile) => Exists(d => d.GetValue("MAST") == masterfile);
        public string[] ToMasterNames() => this.Select(d => d.GetValue("MAST")).ToArray();
    }
}
