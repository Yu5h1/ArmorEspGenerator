using System.Linq;
using System.IO;
using System.Windows;
using Yu5h1Tools.WPFExtension;
using XeLib.API;
using System.Threading.Tasks;
using System;

namespace TESV_EspEquipmentGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] args;

        public App()
        {
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //"G:\Hentai\The Elder Scrolls\TESV\Data\meshes\test"
            //Plugin.CreateNewPlugin(Setup.GameMode.TES5, "TestNewESP.esp", true, (plugin) => {

            //});
            //Shutdown();
            //return;
            args = e.Args;
            if (args.Length == 1)
            {
                var pathinfo = new PathInfo(args[0]);
                if (pathinfo.IsDirectory)
                {
                    var game = Plugin.AssetsFromWhichGame(pathinfo);
                    if (game != null)
                    {
                        if (game == Setup.GameMode.TES5 || game == Setup.GameMode.SSE)
                        {
           
                            Plugin.CreateNewPlugin((Setup.GameMode)game, pathinfo.Name + ".esp", true, (plugin,p) =>
                            {
                                plugin.fileHeader.Author = pathinfo.Name;
                                plugin.GenerateArmorsBySpeculateFolder(pathinfo, progressValue =>
                               {
                                   p.Dispatcher.BeginInvoke(new Action(() =>
                                   {
                                       p.normalizeValue = progressValue;
                                   }));
                                   return false;
                               });
                                return false;
                            });
                        }else "Only support TESV or TESVSE".PromptInfo();
                    }else "Folder path does not exists in Game Data Folder Or the regedit setting of your game is uncurrect.".PromptInfo(); ;
                }
                else
                {
                    switch (pathinfo.extension.ToLower())
                    {
                        case ".nif":
                            if (File.Exists(args[0]))
                            {
                                var bodyparts = PartitionsUtil.ConvertIndicesToBodyParts(Plugin.GetBodyPartsIndicesFromNif(args[0]));
                                bodyparts.ToStringWithNumber().PromptInfo();
                                bodyparts.BSDismemberBodyPartsToPartitions().ToStringWithNumber().PromptInfo();

                            }
                            break;
                        case ".esp":

                            break;
                    }
                }
            }
            else if (args.Length > 1)
            {

            }
            else {
                Win32Util.ForceSingleInstance(this);
                new MainWindow().Show();
            }
            base.OnStartup(e);
        }
    }
}

