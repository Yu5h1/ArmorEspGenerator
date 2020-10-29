using System.Linq;
using System.IO;
using System.Windows;
using Yu5h1Tools.WPFExtension;

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
            //string path = @"G:\Hentai\The Elder Scrolls\TESV\Data\Meshes\test\female\BellaBra_1.nif";
            //NifUtil.GetShapeTexturesByIndex(path, 0).PromptInfo();
            ////var r = NifUtil.GetShapesTextureInfos(path);
            //Shutdown();
            //return;

            args = e.Args;
            if (args.Length == 1) {
                var fileExtension = Path.GetExtension(args[0]).ToLower();
                if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
                {
                    var NIFfiles =  Directory.GetFiles(args[0], "*_1.nif", SearchOption.AllDirectories);
                    foreach (var item in NIFfiles)
                    {
                        
                    }
                }else {
                    switch (fileExtension)
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
            }
            base.OnStartup(e);
        }
    }
}

