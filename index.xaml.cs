using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bilibili_Client
{
    /// <summary>
    /// index.xaml 的交互逻辑
    /// </summary>
    public partial class index : Page
    {
        public index()
        {
            InitializeComponent();
            /*List<Middle> middles = new List<Middle>();
            middles.Add(new Middle("First"));
            middles.Add(new Middle("First"));
            middles.Add(new Middle("First"));
            middles.Add(new Middle("First"));
            content_box.ItemsSource = middles;*/
        }
        public class Middle
        {
            public string video_up { get; private set; }
            public Middle(string fir)
            {
                video_up = fir;
            }
        }
    }
}
