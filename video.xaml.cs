using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Unosquare.FFME.Common;
using static Bilibili_Client.Bilibili_Video;

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
            string currentDirectory = @"C:\Program Files\VideoLAN\VLC";
            var vlcLibDirectory = new DirectoryInfo(currentDirectory);

            var options = new string[]
            {
                //添加日志
                "--file-logging", "-vvv", "--logfile=Logs.log"
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };
            Media2.SourceProvider.CreatePlayer(vlcLibDirectory, options);


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

                ((System.Windows.Shapes.Path)Pause_Button.Content).Data = (Geometry)(converter.ConvertFrom("M744.727273 551.563636L325.818182 795.927273c-30.254545 18.618182-69.818182-4.654545-69.818182-39.563637v-488.727272c0-34.909091 39.563636-58.181818 69.818182-39.563637l418.909091 244.363637c30.254545 16.290909 30.254545 62.836364 0 79.127272z"));
                Is_Play = false;
            }
            else
            {
               
                Media.Play();
                ((System.Windows.Shapes.Path)Pause_Button.Content).Data = (Geometry)(converter.ConvertFrom("M442.181818 709.818182c0 37.236364-30.254545 69.818182-69.818182 69.818182s-69.818182-30.254545-69.818181-69.818182v-395.636364c0-37.236364 30.254545-69.818182 69.818181-69.818182s69.818182 30.254545 69.818182 69.818182v395.636364z m279.272727 0c0 37.236364-30.254545 69.818182-69.818181 69.818182s-69.818182-30.254545-69.818182-69.818182v-395.636364c0-37.236364 30.254545-69.818182 69.818182-69.818182s69.818182 30.254545 69.818181 69.818182v395.636364z"));
                Is_Play = true;
            }
        }
        //这里没整好
        public void Open_New_Video(string avid, MainWindow MainWindow)
        {

            // foreach(string str in )



            Thread Open_New_Video_Thread = new Thread(() =>
             {
                 mainWindow = MainWindow;
                 video_Info = bilibili_Video.gets_video_info(avid);
                 P_data p_data = bilibili_Video.Get_Video_Stream(avid, video_Info.pages[0].cid, 16, IsLogin, biliCookie);

                 int i = 0;
                 List<Choose_Quality_Item> choose_Quality_Items = new List<Choose_Quality_Item>();

                 int clarity = 16;
                 if (mainWindow.IsLogin)
                 {
                     biliCookie = mainWindow.BiliCookie;
                     IsLogin = true;
                    /* clarity = p_data.accept_quality[0];
                     foreach (string str in p_data.accept_description)
                     {

                         choose_Quality_Items.Add(new Choose_Quality_Item(str, p_data.accept_quality[i]));
                         i++;

                     }
                     this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                     {
                         //Choose_Quality.ItemsSource = choose_Quality_Items;
                         Choose_Quality.Items.Add(choose_Quality_Items);
                         Choose_Quality.SelectedIndex = 0;
                     });*/
                 }else
                 {
                    /* foreach (string str in p_data.accept_description)
                     {
                         if (p_data.accept_quality[i] > 32)
                         {
                             i++;
                             continue; 
                         }
                         choose_Quality_Items.Add(new Choose_Quality_Item(str, p_data.accept_quality[i]));
                     }
                     this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                     {
                         // Choose_Quality.ItemsSource = choose_Quality_Items;
                         Choose_Quality.Items.Add(choose_Quality_Items);
                         Choose_Quality.SelectedIndex = Choose_Quality.Items.Count - 1;

                     });*/
                 }
                 if (video_Info.Success)
                     if (p_data.Success)
                         Play_Video(bilibili_Video.Get_Video_Stream(avid, video_Info.pages[0].cid, clarity, IsLogin, biliCookie).url);
             });

            Open_New_Video_Thread.Name = "Open_New_Video_Thread";
            Open_New_Video_Thread.Start();

        }
        private void Change_Quality(object sender, SelectionChangedEventArgs e)
        {
            if (Media.IsOpen)
            {
                P_data p_data = bilibili_Video.Get_Video_Stream(video_Info.aid, video_Info.pages[0].cid, (int)((ComboBoxItem)Choose_Quality.SelectedItem).Uid.ToInt32(), IsLogin, biliCookie);
                if (p_data.Success)
                         Play_Video(p_data.url);
            }
        }
        private void Play_Video(string url)
        {
           /* Media2.SourceProvider.MediaPlayer.Play(new Uri("https://apd-771331d735983f8eb4cd7a9bf2f7502e.v.smtcdns.com/mv.music.tc.qq.com/AkmKcsPzCTN6-ftcAjY1ucIvKiWpS0jYG4uYkHBfYb3k/8E6BCE9A71C642EB3E93FA2301760C6B9E630A5BEEA6B0FC1399DB028B661061825298977B580C3F2DE22AE383EB8B04ZZqqmusic_default/1049_M0118400003epFrw2Uy4CH1001699439.f9844.mp4?fname=1049_M0118400003epFrw2Uy4CH1001699439.f9844.mp4"));*/
            Media.Open(new Uri("https://apd-771331d735983f8eb4cd7a9bf2f7502e.v.smtcdns.com/mv.music.tc.qq.com/AkmKcsPzCTN6-ftcAjY1ucIvKiWpS0jYG4uYkHBfYb3k/8E6BCE9A71C642EB3E93FA2301760C6B9E630A5BEEA6B0FC1399DB028B661061825298977B580C3F2DE22AE383EB8B04ZZqqmusic_default/1049_M0118400003epFrw2Uy4CH1001699439.f9844.mp4?fname=1049_M0118400003epFrw2Uy4CH1001699439.f9844.mp4"));
        }

        private void Change_Speed(object sender, SelectionChangedEventArgs e)
        {
            if (Media.IsOpen)
            {
                Media.SpeedRatio = (double)((ComboBoxItem)Choose_Speed.SelectedItem).Uid.ToDouble();


            }
        }
        public class Choose_Quality_Item
        {
            public string accept_description { get; private set; }
            public int accept_quality { get; private set; }
            public Choose_Quality_Item(string Accept_description, int Accept_quality)
            {
                accept_description = Accept_description;
                accept_quality = Accept_quality;
            }
        }
    }

}
