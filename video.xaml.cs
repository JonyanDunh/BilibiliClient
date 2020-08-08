using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Unosquare.FFME.Common;

namespace Bilibili_Client
{
    /// <summary>
    /// video.xaml 的交互逻辑
    /// </summary>
    /// 

    public class TimeConver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            if (value == null)
                return DependencyProperty.UnsetValue;
            return new Regex(@"(?<time>[0-9]{2}:[0-9]{2}:[0-9]{2}?)", RegexOptions.IgnoreCase).Match(value.ToString()).Groups["time"].Value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    public class Time2Seconds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            if (value == null)
                return DependencyProperty.UnsetValue;
            return ((TimeSpan)value).Hours * 3600 + ((TimeSpan)value).Minutes * 60 + ((TimeSpan)value).Seconds;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

         
            int hour = System.Convert.ToInt32(value) / 3600;
            int min = (System.Convert.ToInt32(value) - hour * 3600) / 60;
            int sen = System.Convert.ToInt32(value) - hour * 3600 - min * 60;
            return new TimeSpan(hour, min, sen);
        }
    }
    public partial class video : Page
    {
        MainWindow mainWindow;
        public bool IsLogin = false;
        Bilibili_Account.BiliCookie biliCookie = new Bilibili_Account.BiliCookie();
        Bilibili_Video bilibili_Video = new Bilibili_Video();
        Bilibili_Video.video_info video_Info = new Bilibili_Video.video_info();
        bool Is_Play = true;
        public video()
        {
            InitializeComponent();
            //


        }

        private void Media_MediaInitializing(object sender, Unosquare.FFME.Common.MediaInitializingEventArgs e)
        {

            if (e.MediaSource.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           e.MediaSource.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                e.Configuration.PrivateOptions["user_agent"] = $"{typeof(ContainerConfiguration).Namespace}/{typeof(ContainerConfiguration).Assembly.GetName().Version}";
                e.Configuration.PrivateOptions["headers"] = "Referer:https://www.bilibili.com";
            }

        }

        //暂停&&播放按钮
        public void Pause(object sender, RoutedEventArgs e)
        {
            
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            if (Is_Play)
            {
                Media.Pause();
                
                ((System.Windows.Shapes.Path)Pause_Button.Content).Data= (Geometry)(converter.ConvertFrom("M744.727273 551.563636L325.818182 795.927273c-30.254545 18.618182-69.818182-4.654545-69.818182-39.563637v-488.727272c0-34.909091 39.563636-58.181818 69.818182-39.563637l418.909091 244.363637c30.254545 16.290909 30.254545 62.836364 0 79.127272z"));
                Is_Play = false;
            }
            else
            {
                Media.Play();
                ((System.Windows.Shapes.Path)Pause_Button.Content).Data = (Geometry)(converter.ConvertFrom("M442.181818 709.818182c0 37.236364-30.254545 69.818182-69.818182 69.818182s-69.818182-30.254545-69.818181-69.818182v-395.636364c0-37.236364 30.254545-69.818182 69.818181-69.818182s69.818182 30.254545 69.818182 69.818182v395.636364z m279.272727 0c0 37.236364-30.254545 69.818182-69.818181 69.818182s-69.818182-30.254545-69.818182-69.818182v-395.636364c0-37.236364 30.254545-69.818182 69.818182-69.818182s69.818182 30.254545 69.818181 69.818182v395.636364z"));
                Is_Play = true;
            }
        }

        public void Open_New_Video(string avid,MainWindow MainWindow)
        {
            new Thread(() =>
            {
                int clarity = 32;
            mainWindow = MainWindow;
            video_Info = bilibili_Video.gets_video_info(avid);
            if (mainWindow.IsLogin)
            {
                biliCookie = mainWindow.BiliCookie;
                IsLogin = true;
                clarity = 80;
            }
            if (video_Info.Success)
                if(bilibili_Video.Get_Video_Stream(avid, video_Info.pages[0].cid, 80, IsLogin, biliCookie).Success)
                    Play_Video(bilibili_Video.Get_Video_Stream(avid, video_Info.pages[0].cid,80, IsLogin, biliCookie).url);

            }).Start();
        }
        private void Play_Video(string url)
        {

            Media.Open(new Uri(url));
        }

    }
    
}
