using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unosquare.FFME.Common;
using MessageBox = System.Windows.Forms.MessageBox;

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
        bool Is_Play = true;
        public video()
        {
            InitializeComponent();
            //Media.Open(new Uri(@"http://upos-sz-mirrorcos.bilivideo.com/upgcxcode/02/34/186803402/186803402-1-112.flv?e=ig8euxZM2rNcNbN1nwuVhwdlhbR37zdVhoNvNC8BqJIzNbfqXBvEqxTEto8BTrNvN0GvT90W5JZMkX_YN0MvXg8gNEV4NC8xNEV4N03eN0B5tZlqNxTEto8BTrNvNeZVuJ10Kj_g2UB02J0mN0B5tZlqNCNEto8BTrNvNC7MTX502C8f2jmMQJ6mqF2fka1mqx6gqj0eN0B599M=&uipk=5&nbs=1&deadline=1596880701&gen=playurl&os=cosbv&oi=3070377771&trid=a37775b2f77d46db81ba04bf1a7aabe1u&platform=pc&upsig=71125246097ec0f0c16b669820cbc2ec&uparams=e,uipk,nbs,deadline,gen,os,oi,trid,platform&mid=96876893&orderid=0,3&agrr=1&logo=80000000"));


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
        private void Pause(object sender, RoutedEventArgs e)
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

        public void Open_New_Video(string avid)
        {

            MessageBox.Show(avid);

        }


    }
    
}
