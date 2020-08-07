using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

namespace Bilibili_Client
{
    /// <summary>
    /// video.xaml 的交互逻辑
    /// </summary>
    /// 

    
    public partial class video : Page
    {
        bool Is_Play = true;
        public video()
        {
            InitializeComponent();
            Media.Open(new Uri(@"http://upos-sz-mirrorcos.bilivideo.com/upgcxcode/56/11/182011156/182011156_da2-1-112.flv?e=ig8euxZM2rNcNbhVhb4BhwdlhzdgnwUVhoNvNC8BqJIzNbfqXBvEqxTEto8BTrNvN0GvT90W5JZMkX_YN0MvXg8gNEV4NC8xNEV4N03eN0B5tZlqNxTEto8BTrNvNeZVuJ10Kj_g2UB02J0mN0B5tZlqNCNEto8BTrNvNC7MTX502C8f2jmMQJ6mqF2fka1mqx6gqj0eN0B599M=&uipk=5&nbs=1&deadline=1596811742&gen=playurl&os=cosbv&oi=3070377771&trid=c2a76f0ba9bc4ebc922998511eebb807u&platform=pc&upsig=40bc8ea9b7d6a44769aef5b914f0c140&uparams=e,uipk,nbs,deadline,gen,os,oi,trid,platform&mid=96876893&orderid=0,3&agrr=0&logo=80000000"));
            
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



        private void Pause(object sender, RoutedEventArgs e)
        {
           // System.Windows.MessageBox.Show("" + PreviewSlider.PreviewPosition);
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            if (Is_Play)
            {
                Media.Pause();
                
                ((System.Windows.Shapes.Path)Pause_Button.Content).Data= (Geometry)(converter.ConvertFrom("M178.761143 11.702857l36.132571 25.307429 646.619429 452.169143a43.885714 43.885714 0 0 1-0.841143 72.521142L177.883429 1015.296a43.885714 43.885714 0 0 1-68.169143-36.571429V47.689143A43.885714 43.885714 0 0 1 178.761143 11.702857z m1.316571 916.809143L788.114286 524.544 180.077714 99.328v829.184z"));
                Is_Play = false;
            }
            else
            {
                Media.Play();
                ((System.Windows.Shapes.Path)Pause_Button.Content).Data = (Geometry)(converter.ConvertFrom("M735.9 64.9c-51.2 0-96 44.7-96 95.8v702.6c0 51.1 44.8 95.8 96 95.8s96-44.7 96-95.8V160.7c0-51.1-44.8-95.8-96-95.8z m-447.8 0c-51.2 0-96 44.7-96 95.8v702.6c0 51.1 44.8 95.8 96 95.8s96-44.7 96-95.8V160.7c-0.1-51.1-44.9-95.8-96-95.8z"));
                Is_Play = true;
            }
        }
    }
}
