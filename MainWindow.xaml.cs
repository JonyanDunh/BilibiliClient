using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Bilibili_Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 实例化计时器
        private DispatcherTimer showTimer = new DispatcherTimer();

        index index_page = new index();
        login login_page = new login();
        geetest geetest_page = new geetest();

        private delegate void MainWindow_SendMessage_To_Login_page();//主窗口发送给登录窗口类
        private MainWindow_SendMessage_To_Login_page mainWindow_SendMessage_To_Login_page;//
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
            

            Message();


            middle_frame.Navigate(login_page);

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
        private void Message()
        {
            login_page.login_sendMessage_To_Mainwindow = MainWindow_Recevie_From_Login_Page;//把登录页面的发送函数和接受函数链接
            mainWindow_SendMessage_To_Login_page = login_page.Login_Recevie_From_Mainwindow;//把发送函数与登录页面的接收函数链接
            login_page.login_open_geetest_page = login_open_geetest_page;//登录页面控制主窗口打开验证页面

            geetest_page.geetest_SendKey_To_Login_page = login_page.Login_Recevie_Key_From_Geetest_page;//把验证页面的发送Key函数和登录页面接受Key函数链接
            geetest_page.geetest_SendSmsKey_To_Login_page = login_page.Login_Recevie_SmsKey_From_Geetest_page;//把验证页面的发送SmsKey函数和登录页面接受SmsKey函数链接

            login_page.sendKey_To_Geetest_page = geetest_page.Geetest_Get_Key_From_Login_Page;////把登录页面的发送函数和验证页面接受函数链接

        }
        private void MainWindow_Recevie_From_Login_Page()
        {


        }
        private void login_open_geetest_page()
        {
            right_frame.Navigate(geetest_page);            
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

            mainWindow_SendMessage_To_Login_page();



        }

        public class left_button_class
        {
            public string left_button_img { get; private set; }
            public string left_button_content { get; private set; }



            public left_button_class(
                string button_img,
                string button_content
                )
            {
                left_button_img = button_img;
                left_button_content = button_content;

            }
        }

        private void textBox1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }
        // 定时器 GC
        private void TimerGC(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
    }
}
