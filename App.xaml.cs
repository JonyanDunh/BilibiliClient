using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME;

namespace Bilibili_Client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //if (Environment.Is64BitProcess)
            //    Library.FFmpegDirectory = @"resource\FFmpeg\x64";
            //else
            //    Library.FFmpegDirectory = @"resource\FFmpeg\x86";
            Library.FFmpegDirectory = @"resource\FFmpeg\";
            Library.LoadFFmpeg();
            MediaElement.FFmpegMessageLogged += (s, ev) =>
            {
                System.Diagnostics.Debug.WriteLine(ev.Message);
            };
            base.OnStartup(e);

        }
    }
    
}
