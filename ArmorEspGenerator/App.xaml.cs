﻿using System.IO;
using System.Windows;
using XeLib;
using XeLib.API;
using static XeLib.API.Setup;
using static InformationViewer;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace TESV_EspEquipmentGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] args;
        public static bool LaunchWithoutWindow = false;
        protected override void OnStartup(StartupEventArgs e)
        {
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
                                var bodyparts = PartitionsUtil.ConvertIndicesToBodyParts(Plugin.GetBodyPartsIDsFromNif(args[0]));
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
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // Bring window to foreground
            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }

            MainWindow.Activate();

            return true;
        }
    }
}
