using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Bilibili_Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow3
    { 

    }

    public partial class MainWindow : Window
    {
        // 实例化计时器
        private DispatcherTimer showTimer = new DispatcherTimer();
        index index_page = new index();
        public MainWindow()
        {
            // 定义定时器,TimeSpan最后一个值是时间，单位秒，3表示3秒检测一次
            showTimer.Tick += new EventHandler(TimerGC);
            showTimer.Interval = new TimeSpan(0, 0, 0, 1);
            showTimer.Start();
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;//显示位置屏幕居中
            mainwindow.Width = (SystemParameters.PrimaryScreenWidth) * 0.83;
            mainwindow.Height = (SystemParameters.PrimaryScreenHeight) * 0.83;
            left_grid.Width = mainwindow.Width * 0.145;
            right_grid.Width = mainwindow.Width * 0.234;    
            middle_frame.Navigate(index_page);
            if (false == Directory.Exists(@"Data\Cache"))
            {
                //创建DATA文件夹
                Directory.CreateDirectory(@"Data\Cache");
            }
            List<left_button_class> Left_button_class = new List<left_button_class>();
            Left_button_class.Add(new left_button_class("resource/img/首页.png", "首页"));
            Left_button_class.Add(new left_button_class("resource/img/动态.png", "动态"));
            Left_button_class.Add(new left_button_class("resource/img/排行.png", "排行"));
            Left_button_class.Add(new left_button_class("resource/img/分类.png", "分类"));
            Left_button_class.Add(new left_button_class("resource/img/直播.png", "直播"));
            Left_button_class.Add(new left_button_class("resource/img/番剧.png", "番剧"));
            Left_button_class.Add(new left_button_class("resource/img/漫画.png", "漫画"));
            Left_button_class.Add(new left_button_class("resource/img/会员购.png", "会员购"));
            Left_button_class.Add(new left_button_class("resource/img/游戏中心.png", "游戏中心"));
            Left_button_class.Add(new left_button_class("resource/img/投稿工具.png", "投稿工具"));
            left_button_box.ItemsSource = Left_button_class;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)//移动窗口
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }



        private void index_button(object sender, RoutedEventArgs e)
        {

            GC.Collect();
            GC.WaitForPendingFinalizers();



        }

        public class left_button_class
        {
            public string left_button_img { get; private set; }
            public string left_button_name { get; private set; }



            public left_button_class(
                string button_img, 
                string button_name
                )
            {
                left_button_img = button_img;
                left_button_name = button_name;

            }
        }

        private void textBox1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            

            /*search_logo.BorderThickness.Left = 1;
            search_logo.BorderThickness.Top = 1;
            search_logo.BorderThickness.Right = 0;
            search_logo.BorderThickness.Bottom = 1;
            search_textbox_border.BorderThickness*/
        }
        // 定时器 GC
        private void TimerGC(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
