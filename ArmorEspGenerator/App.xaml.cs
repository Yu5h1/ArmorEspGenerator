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

                                Plugin.CreateNewPlugin((Setup.GameMode)game, pathinfo.Name + ".esp", true, (plugin, progressInfo) =>
                                {
                                    plugin.fileHeader.Author = pathinfo.Name;
                                    plugin.GenerateArmorsBySpeculateFolder(pathinfo, progressValue =>
                                    {
                                        progressInfo.SetProcessIcon(0.1 + (progressValue * 0.8));
                                        progressInfo.Text = "....." + (progressValue * 100).ToString() + "%";
                                        return false;
                                    });

                                    return false;
                                });
                            }
                            else
                            {
                                "Only support TESV or TESVSE".PromptWarnning();
                                Shutdown();
                            }

                        }
                        else
                        {
                            "Folder path does not exists in Game Data Folder Or the regedit setting of your game is uncurrect.".PromptWarnning();
                            Shutdown();
                        }
                    }
                } else {
                    "Only one parameter is allowed".PromptWarnning();
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

