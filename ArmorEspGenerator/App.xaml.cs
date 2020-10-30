using System.Linq;
using System.IO;
using System.Windows;
using Yu5h1Tools.WPFExtension;
using XeLib.API;

namespace TESV_EspEquipmentGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] args;
        public static bool LaunchWithoutWindow = false;

        public App()
        {
            Win32Util.ForceSingleInstance(this);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //Plugin.CreateNewPlugin(Setup.GameMode.TES5, "TestNewESP.esp", true, (plugin) => {

            //});
            //Shutdown();
            //return;
            args = e.Args;
            if (args.Length == 1) {
                var pathinfo = new PathInfo(args[0]);
                if (pathinfo.IsDirectory)
                {
                    var game = Plugin.AssetsFromWhichGame(pathinfo);
                    if (game != null)
                    {
                        if (game == Setup.GameMode.TES5 || game == Setup.GameMode.SSE) {
                            Plugin.CreateNewPlugin((Setup.GameMode)game, pathinfo.Name + ".esp", true, (plugin) =>
                            {
                                plugin.GenerateArmorsBySpeculateFolder(pathinfo);
                            });
                        }

                    }
                    else "Folder path does not exists in Game Data Folder".PromptInfo(); ;
                }
                else {
                    switch (pathinfo.extension.ToLower())
                    {
                        case ".nif":
                            LaunchWithoutWindow = true;
                            if (File.Exists(args[0])) {
                                var bodyparts = PartitionsUtil.ConvertIndicesToBodyParts(Plugin.GetBodyPartsIndicesFromNif(args[0]));
                                bodyparts.ToStringWithNumber().PromptInfo();
                                bodyparts.BSDismemberBodyPartsToPartitions().ToStringWithNumber().PromptInfo();

                            }
                            break;
                        case ".esp":
  
                            break;
                    }
                }
                Shutdown();
            }else if (args.Length > 1)
            {

            }
            base.OnStartup(e);
        }
    }
}

