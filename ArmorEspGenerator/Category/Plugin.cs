using System;
using System.IO;
using System.Collections.Generic;
using XeLib.API;
using XeLib;
using static InformationViewer;
using System.Text.RegularExpressions;

namespace TESV_EspEquipmentGenerator
{
    public partial class Plugin : RecordElement
    {
        public static Setup.GameMode CurrentGameMode;

        public override string signature => "TES4";

        public string PluginName;
        public string FullPath => GetPluginFullPath(PluginName);
        public string GetTempName() => PluginName.ToLower().Replace(".esp", "_temp.esp");
        public string GetTempFullPath() => GetPluginFullPath(GetTempName());
        public static string GetTempName(string name) => name.ToLower().Replace(".esp", "_temp.esp");
        public static string GetTempFullPath(string name) => GetPluginFullPath(GetTempName(name));
        

        Plugin(Handle target, string pluginName) : base(target) {
            PluginName = pluginName;
            TextureSets = new PluginRecords<TextureSet>(this, TextureSet.Create);
            ArmorAddons = new PluginRecords<ArmorAddon>(this, ArmorAddon.Create);
            Armors = new PluginRecords<Armor>(this, Armor.Create);
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
            if (LoadPlugins(gameMode, espName))
                return new Plugin(Elements.GetElement(Handle.BaseHandle, espName), pluginName);
            else
                return null;
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
                using (var p = new Yu5h1Tools.WPFExtension.WaitCursorProcess())
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

        public static string GetGamePath() => Setup.GetGamePath(CurrentGameMode);        
        public static String GetGameDataPath(Setup.GameMode gameMode) => Path.Combine(Setup.GetGamePath(gameMode), "Data");
        public static String GetGameDataPath() => Path.Combine(GetGamePath(), "Data");
        public static String GetGameDataPath(string suffix) => GetGameDataPath().CombineNoleadSlash(suffix).ReplaceRepeatFolderUtilEmpty("Data");

        public static String GetMeshesPath() => Path.Combine(GetGameDataPath(), "Meshes");
        public static String GetMeshesPath(string suffix) =>
                                GetMeshesPath().CombineNoleadSlash(suffix).ReplaceRepeatFolderUtilEmpty("Meshes");

        public static String GetTexturesPath() => Path.Combine(GetGameDataPath(), "Textures");
        public static String GetTexturesPath(string suffix) =>
                                GetTexturesPath().CombineNoleadSlash(suffix).ReplaceRepeatFolderUtilEmpty("textures");
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

        public static bool printLoadingLog = false;
        public static bool LoadPlugins(Setup.GameMode gameMode, params string[] PluginsList)
        {
            //Meta.Release(Handle.BaseHandle);
            if (printLoadingLog) print("Initializing XEditLib");
            Meta.Initialize();
            if (printLoadingLog) print("Setting game mode to " + gameMode.ToString());
            Setup.SetGameMode(gameMode);
            if (printLoadingLog) print("Loading plugins " + string.Join("&", PluginsList));
            try
            {
                Setup.LoadPlugins(string.Join("\n", PluginsList));
            }
            catch (Exception error)
            {
                error.Message.PromptWarnning();
                return false;
            }
            if (printLoadingLog) print("Waiting for loader to finish");
            var state = Setup.LoaderState.IsInactive;
            while (state != Setup.LoaderState.IsDone && state != Setup.LoaderState.HasError) state = Setup.GetLoaderStatus();
            if (printLoadingLog)
            {
                print("printing XEditLib output");
                print(Messages.GetMessages());
            }
            Messages.ClearMessages();
            print("Loader finished");
            return true;
        }
        
    }
}
