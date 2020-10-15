using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using XeLib.API;
using XeLib;
using static InformationViewer;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public partial class Plugin : RecordElement
    {
        public static Plugin current;

        public static string GetLoadedGameName()
        { try { return Meta.GetGlobal("AppName"); } catch (Exception) { return ""; } }
        public static bool IsLoaded => GetLoadedGameName() != string.Empty;

        public static Setup.GameMode ParseGameMode(string modeName) =>
                            (Setup.GameMode)Enum.Parse(typeof(Setup.GameMode), modeName);
        public static Setup.GameMode ActiveGameMode => ParseGameMode(GetLoadedGameName());

        public override string signature => "TES4";

        public string PluginName;
        public string FullPath => GetPluginFullPath(PluginName);
        public string GetTempName() => PluginName.ToLower().Replace(".esp", "_temp.esp");
        public string GetTempFullPath() => GetPluginFullPath(GetTempName());
        public static string GetTempName(string name) => name.ToLower().Replace(".esp", "_temp.esp");
        public static string GetTempFullPath(string name) => GetPluginFullPath(GetTempName(name));

        public FileHeader fileHeader;
        public PluginMasters pluginMasters;

        void Init(string pluginName) {
            PluginName = pluginName;
            TextureSets = new PluginRecords<TextureSet>(this, TextureSet.Create);
            ArmorAddons = new PluginRecords<ArmorAddon>(this, ArmorAddon.Create);
            Armors = new PluginRecords<Armor>(this, Armor.Create);
            fileHeader = new FileHeader(handle);
            pluginMasters = new PluginMasters(fileHeader);
        }

        public Plugin(string pluginName, params string[] masterfiles) : base(Handle.BaseHandle, Handle.BaseHandle.AddElement(pluginName))
        {
            Init(pluginName);
            pluginMasters.Add(masterfiles);
        }

        Plugin(Handle target, string pluginName) : base(Handle.BaseHandle,target) {
            Init(pluginName);
        }
        

        public static Plugin Load(Setup.GameMode gameMode, string pluginName, bool AsTemp = false)
        {
            string espName = pluginName;
            var fullPluginPath = GetPluginFullPath(pluginName);
            if (!File.Exists(fullPluginPath)) return null;
            if (AsTemp)
            {
                espName = GetTempName(pluginName);
                var tempPath = GetPluginFullPath(espName);
                if (IsFileNotLockedElsePrompt(tempPath))
                    File.Copy(fullPluginPath, tempPath, true);
            }
            if (LoadPlugins(gameMode, espName)) {
                current = new Plugin(Elements.GetElement(Handle.BaseHandle, espName), pluginName);
                return current;
            }else return null;
        }

        public string Author { get => GetValue(@"File Header\CNAM"); set => SetValue(@"File Header\CNAM", value); }
        public string Description { get => GetValue(@"File Header\SNAM"); set => SetValue(@"File Header\SNAM", value); }

        public Handle[] GetRecords(bool includeOverrides = false) => Records.GetRecords(handle, "", includeOverrides);
        public PluginRecords<TextureSet> TextureSets;
        public PluginRecords<Armor> Armors;
        public PluginRecords<ArmorAddon> ArmorAddons;
        public bool Save() {
            if (IsFileNotLockedElsePrompt(FullPath))
            {
                using (var p = new WaitCursorProcess())
                {
                    Files.SaveFile(handle, FullPath);
                    p.PlayCompletedSound = true;
                }
                return true;
            }
            return false;
        }
        public void UnLoad() {
            if (File.Exists(GetTempFullPath())) {
                //var loadedFiles = Setup.GetLoadedFileNames(true);
                //for (int i = loadedFiles.Length - 1; i >= 0; i--)
                //{
                //    handle.GetElement(Handle)
                //}
                //Setup.UnloadPlugin(handle);
                Meta.Close();
                //Meta.Release(Handle.BaseHandle);
                File.Delete(GetTempFullPath());
            }
        }

        public static string GetGamePath() {
            if (IsLoaded) return Setup.GetGamePath(ActiveGameMode);
            return "";
        } 
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
            if (!result) (GetGameDataPath()+"\n\n"+path+"\n\nThe Drop files are not locate at Selected Game's folder").PromptWarnning();
            return result;
        }

        public static bool ContainDataFolderInPath(string path) => path.ToLower().Contains(@"\data\");
        public static bool ContainTexturesFolderInPath(string path) => path.ToLower().Contains(@"\textures\");
        public static bool ContainMeshesFolderInPath(string path) => path.ToLower().Contains(@"\meshes\");
        public static  string[] GetMasterNames(string PluginName) {
            var results = new string[0];
            if (File.Exists(GetPluginFullPath(PluginName))) {
                var header = Setup.LoadPluginHeader(PluginName);
                var masters = header.GetElements()[0].GetElement("Master Files").GetElements();
                results = new string[masters.Length];
                for (int i = 0; i < results.Length; i++)
                    results[i] = masters[i].GetValue("MAST");
                Setup.UnloadPlugin(header);
            }
            return results;
        }
        
        public static string[] GetMissingMastersInfo(params string[] PluginsList) {
            List<string> missingMasters = new List<string>();

            foreach (var pluginName in PluginsList)
            {
                if (File.Exists(GetPluginFullPath(pluginName)))
                {
                    var masters = GetMasterNames(pluginName);
                    foreach (var master in masters)
                    {
                        if (!File.Exists(GetPluginFullPath(master))) {
                            if (!missingMasters.Contains(master))
                                missingMasters.Add(master);
                        }
                    }
                }
                else if (!missingMasters.Contains(pluginName))
                    missingMasters.Add(pluginName);
            }
            return missingMasters.ToArray();
        }
        public static Handle GetActivePlugin(string pluginName)
                                                    => Handle.BaseHandle.GetElement(pluginName);
        public static Handle[] GetActivePluginRecords(string pluginName,string search = "",bool includeOverrides = false)
                                            => GetActivePlugin(pluginName).GetRecords(search,includeOverrides);
        public static void Reset() {
            //Meta.ResetStore();
            Meta.Initialize();
        }
        public static bool SetGameMode(Setup.GameMode gameMode) {
            if (IsLoaded) MainWindow.current.ReStartApplication();
            Reset();
            try
            {
                Setup.SetGameMode(gameMode);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static bool LoadPlugins(Setup.GameMode gameMode, params string[] PluginsList)
        {
            var missingMasters = GetMissingMastersInfo(PluginsList);
            if (missingMasters.Length > 0) {
                ("The dependent modules are missing : \n" + missingMasters.Join("\n")).PromptWarnning();
                return false;
            }
            try
            {
                Setup.LoadPlugins(string.Join("\n", PluginsList));
            }
            catch (Exception error)
            {
                
                error.Message.PromptWarnning();
                return false;
            }
            var state = Setup.LoaderState.IsInactive;
            while (state != Setup.LoaderState.IsDone && state != Setup.LoaderState.HasError) state = Setup.GetLoaderStatus();
            Messages.ClearMessages();
            return true;
        }
        public static bool CreateNewPlugin(string pluginName, params string[] masterfiles) {
            if (File.Exists(GetPluginFullPath(pluginName))) {
                "Plugin already exists".PromptWarnning();
                return false;
            }
            var newPlugin = new Plugin(pluginName, masterfiles);
            return true;
        }
    }
    public class FileHeader : RecordObject
    {
        public static string Signature => "File Header";
        public override string signature => Signature;
        public FileHeader(Handle Parent) : base(Parent) {}

    }
    public class PluginMasters : RecordArrayObject<Handle>
    {
        public static string Signature => "Master Files";
        public override string signature => Signature;
        public PluginMasters(FileHeader fileHeader) : base(fileHeader.handle) {
            AddRange(handle.GetElements());
        }
        public void Add(params string[] masterFiles)
        {
            foreach (var mast in masterFiles)
            {
                if (!Exists(mast)) {
                    var newItem = handle.AddElement();
                    newItem.SetValue("MAST", mast);
                }
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
