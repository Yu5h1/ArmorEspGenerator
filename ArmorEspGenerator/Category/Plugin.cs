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
        public static Handle BaseHandle
        {
            get {
                return Handle.BaseHandle;
            } 
        }
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
            TextureSets = new PluginRecords<TextureSet>(this, TextureSet.Create);
            ArmorAddons = new PluginRecords<ArmorAddon>(this, ArmorAddon.Create);
            Armors = new PluginRecords<Armor>(this, Armor.Create);
            fileHeader = new FileHeader(handle);            
        }

        public Plugin(string pluginName, params string[] masterfiles) : base(BaseHandle, BaseHandle.AddElement(pluginName))
        {
            Init(pluginName);
            fileHeader.masters.Add(masterfiles);
        }

        Plugin(Handle target, string pluginName) : base(Handle.BaseHandle,target) {
            Init(pluginName);
        }
        

        public static Plugin Load(string pluginName, bool AsTemp = false)
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
            if (LoadPlugins( espName)) {
                current = new Plugin(Elements.GetElement(Handle.BaseHandle, espName), pluginName);
                return current;
            }else return null;
        }

        public string Author { get => GetValue(@"File Header\CNAM"); set => SetValue(@"File Header\CNAM", value); }
        public string Description { get => GetValue(@"File Header\SNAM"); set => SetValue(@"File Header\SNAM", value); }

        public Handle[] GetRecords(string recordSignature = "", bool includeOverrides = false) => Records.GetRecords(handle, recordSignature, includeOverrides);

        public PluginRecords<TextureSet> TextureSets;
        public PluginRecords<Armor> Armors;
        public PluginRecords<ArmorAddon> ArmorAddons;


        public bool Save() {
            if (IsFileNotLockedElsePrompt(FullPath))
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
            if (!result) (GetGameDataPath()+"\n\n"+path+"\n\nThe Drop files are not locate at Selected Game's folder").PromptWarnning();
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
                    var masters = header.GetElements()[0].GetElement("Master Files").GetElements();
                    results = new string[masters.Length];
                    for (int i = 0; i < results.Length; i++)
                        results[i] = masters[i].GetValue("MAST");
                    Setup.UnloadPlugin(header);
                }
                catch (Exception error)
                {
                    error.Message.PromptError();
                }
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
        public static bool SetGameMode(Setup.GameMode gameMode) {
            
            try
            {
                currentGameMode = gameMode;
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
        public static bool CreateNewPlugin(
            Setup.GameMode gameMod,string pluginName,bool overwrite,Action<Plugin> workflow, params string[] masterfiles) {

            Meta.Initialize();
            Setup.SetGameMode(gameMod);
            var fullPathInfo = new PathInfo(Setup.GetGamePath(gameMod) + @"\Data\" + pluginName);
            if (IsFileLockedPrompt(fullPathInfo)) return false;
            
            if (File.Exists(fullPathInfo) && !overwrite) {
                (fullPathInfo + " already exists ! ").PromptWarnning();
                return false;
            }
            Setup.LoadPlugins(masterfiles.Join("\n"));
            var state = Setup.LoaderState.IsInactive;
            while (state != Setup.LoaderState.IsDone && state != Setup.LoaderState.HasError) state = Setup.GetLoaderStatus();
            Messages.ClearMessages();

            if (File.Exists(fullPathInfo)) File.Delete(fullPathInfo);

            var pluginFileHandle = Files.AddFile(pluginName);
            var newPlugin = new Plugin(pluginName,masterfiles);
            workflow(newPlugin);
            Files.SaveFile(pluginFileHandle, fullPathInfo);

            return true;
        }
    }
    public class FileHeader : RecordObject
    {
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
        public FileHeader(Handle Parent) : base(Parent) {
            masters = new PluginMasters(this);
        }

    }
    public class PluginMasters : RecordArrays<Handle>
    {
        public static string Signature => "Master Files";
        public override string signature => Signature;
        public PluginMasters(FileHeader fileHeader) : base(fileHeader.handle) {
            if (fileHeader.handle == null ) {
                "FileHeader is null".PromptWarnning();
            }
            if (handle == null) {
                parent.AddArrayItem(signature, "MAST", "Skyrim.esm");
            }
            AddRange(handle.GetElements());
        }
        public void Add(params string[] masterFiles)
        {
            if (masterFiles.Length > 0) {
                foreach (var mast in masterFiles)
                {
                    if (!Exists(mast))
                    {
                        var newItem = handle.AddElement();
                        newItem.SetValue("MAST", mast);
                    }
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
