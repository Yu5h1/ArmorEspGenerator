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
            //Plugin.SetGameMode(Setup.GameMode.TES5);
            //Plugin.LoadPlugins("Skyrim.esm");
            
            ////p.handle.GetDisplayName().PopupInfo();
            ////"G:\Hentai\The Elder Scrolls\TESV\Data\meshes\test"
            ////Plugin.CreateNewPlugin(Setup.GameMode.TES5, "TestNewESP.esp", true, (plugin) => {
            ////});
            //Shutdown();
            //return;

            args = e.Args;
            if (args.Length > 0)
            {
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
                                try
                                {
                                    Plugin.CreateNewPlugin((Setup.GameMode)game, pathinfo.Name + ".esp", (plugin, progressInfo) =>
                                    {
                                        plugin.fileHeader.Author = pathinfo.Name;
                                        return plugin.GenerateArmorsBySpeculateFolder(pathinfo, progressValue =>
                                        {
                                            progressInfo.SetProcessIcon(0.1 + (progressValue * 0.8));
                                            progressInfo.Text = "....." + (progressValue * 100).ToString() + "%";
                                            return false;
                                        });
                                    }, "Skyrim.esm");
                                } catch (Exception error)
                                {
                                    error.Message.PopupError();
                                    throw;
                                }
       
                            }
                            else
                            {
                                "Only support TESV or TESVSE".PopupWarnning();
                                Shutdown();
                            }

                        }
                        else
                        {
                            "Folder path does not exists in Game Data Folder Or the regedit setting of your game is uncurrect.".PopupWarnning();
                            Shutdown();
                        }
                    }
                } else {
                    "Only one parameter is allowed".PopupWarnning();
                    Shutdown();
                }
            }
            else {
                Win32Util.ForceSingleInstance(this);
                new MainWindow().Show();
            }
            base.OnStartup(e);
        }
    }
}

