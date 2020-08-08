using HandyControl.Controls;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Bilibili_Client
{


    public partial class MainWindow : System.Windows.Window
    {

        // 实例化计时器
        private DispatcherTimer GCTimer = new DispatcherTimer();
        index index_page = new index();
        login login_page = new login();
        Space space_page = new Space();
        video video_page = new video();
        geetest geetest_page = new geetest();
        Bilibili_Account bilibili = new Bilibili_Account();
        public Bilibili_Account.BiliCookie BiliCookie = new Bilibili_Account.BiliCookie();
        public bool IsLogin = false;
        private delegate void MainWindow_SendMessage_To_Login_page();//主窗口发送给登录窗口类
        private MainWindow_SendMessage_To_Login_page mainWindow_SendMessage_To_Login_page;

        private delegate void MainWindow_Open_Video_page(string avid,MainWindow mainWindow);//主窗口发送给登录窗口类
        private MainWindow_Open_Video_page mainWindow_Open_Video_page;

        public MainWindow()
        {
            InitializeComponent();
             initialization();//初始化内容
             Adaptive();//屏幕自适应
             Message();//建立信息槽
             Create_data();//创建文件夹

        }

        //初始化内容
        private void initialization()
        {
            //初始化定时器，定时强制GC回收
            GCTimer.Tick += new EventHandler(TimerGC);
            GCTimer.Interval = new TimeSpan(0, 0, 0, 1);
            GCTimer.Start();
            middle_frame.Navigate(index_page);//默认打开主页
            if (bilibili.Check_Login_status())//判断是否登录
            {
                IsLogin = true;
                bilibili.Set_User_Data(this);
            }
            Last_sideMenuItem = Index_Button;
            Last_animationPath =(AnimationPath)Index_Button.Icon;
            Last_sideMenuItem.Foreground = new SolidColorBrush(Color.FromRgb(251, 114, 153));
            Last_animationPath.Stroke = new SolidColorBrush(Color.FromRgb(251, 114, 153));
            Last_animationPath.IsPlaying = true;
        }

        //屏幕自适应
        private void Adaptive()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;//显示位置屏幕居中

        }

        //建立信息槽
        private void Message()
        {
            login_page.Login_Success = Login_Success;//把登陆成功信号发给主窗口
            login_page.login_sendMessage_To_Mainwindow = MainWindow_Recevie_From_Login_Page;//把登录页面的发送函数和接受函数链接
            mainWindow_SendMessage_To_Login_page = login_page.Login_Recevie_From_Mainwindow;//把发送函数与登录页面的接收函数链接
            login_page.login_open_geetest_page = login_open_geetest_page;//登录页面控制主窗口打开验证页面
            geetest_page.geetest_SendKey_To_Login_page = login_page.Login_Recevie_Key_From_Geetest_page;//把验证页面的发送Key函数和登录页面接受Key函数链接
            geetest_page.geetest_SendSmsKey_To_Login_page = login_page.Login_Recevie_SmsKey_From_Geetest_page;//把验证页面的发送SmsKey函数和登录页面接受SmsKey函数链接
            login_page.sendKey_To_Geetest_page = geetest_page.Geetest_Get_Key_From_Login_Page;////把登录页面的发送函数和验证页面接受函数链接
            index_page.index_Page_Open_Video = Open_Video_Page;
            mainWindow_Open_Video_page=video_page.Open_New_Video;
        }

        //创建文件夹
        private void Create_data()
        {
            if (false == Directory.Exists(@"Data\Cache\Img\Cover"))
            {
                Directory.CreateDirectory(@"Data\Cache\Img\Cover");
            }

        }
        //主窗口从登录页面传回来的数据
        private void MainWindow_Recevie_From_Login_Page()
        {


        }

        //登录成功后操作
        private void Login_Success()
        {
            IsLogin = true;
            bilibili.Set_User_Data(this);
            middle_frame.Navigate(space_page);
            middle_title.Text = bilibili.Get_User_Data("uname", "", false, this) + "的个人空间";
            geetest_page.geetest_web.Dispose();
            geetest_page.Close();
            geetest_page = null;
            login_page = null;

        }
        //登录页面打开极验页面
        private void login_open_geetest_page()
        {
            geetest_page.Show();
        }
        private void Open_Video_Page(string avid)
        {
            middle_frame.Navigate(video_page);
            mainWindow_Open_Video_page(avid,this);
            

        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//移动窗口
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        //主页按钮
        private void Index_Refresh_button(object sender, RoutedEventArgs e)
        {

            mainWindow_SendMessage_To_Login_page();
        }
        // 定时器 GC
        private void TimerGC(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        //点击头像
        private void Open_User_Space(object sender, MouseButtonEventArgs e)
        {
            if (IsLogin)
            {
                middle_frame.Navigate(space_page);
                middle_title.Text = bilibili.Get_User_Data("uname", "", false, this)+"的个人空间";
               

            }
            else
            {
                middle_frame.Navigate(login_page);
                middle_title.Text ="登录";
            }
                Last_sideMenuItem.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                Last_animationPath.IsPlaying = false;
                Last_animationPath.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        }
        //打开首页的按钮
        private void Open_Index(object sender, MouseButtonEventArgs e)
        {
            middle_frame.Navigate(index_page);
            middle_title.Text = "首页";
            
        }


        //登录按钮
        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            middle_frame.Navigate(login_page);
            middle_title.Text = "登录";

                Last_sideMenuItem.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                Last_animationPath.IsPlaying = false;
                Last_animationPath.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        //页面返回按钮
        private void Black(object sender, RoutedEventArgs e)
        {
            if (middle_frame.CanGoBack)
            {
                if (video_page.Media.IsPlaying)
                {
                    var converter = TypeDescriptor.GetConverter(typeof(Geometry));
                    video_page.Media.Pause();
                    ((System.Windows.Shapes.Path)video_page.Pause_Button.Content).Data = (Geometry)(converter.ConvertFrom("M442.181818 709.818182c0 37.236364-30.254545 69.818182-69.818182 69.818182s-69.818182-30.254545-69.818181-69.818182v-395.636364c0-37.236364 30.254545-69.818182 69.818181-69.818182s69.818182 30.254545 69.818182 69.818182v395.636364z m279.272727 0c0 37.236364-30.254545 69.818182-69.818181 69.818182s-69.818182-30.254545-69.818182-69.818182v-395.636364c0-37.236364 30.254545-69.818182 69.818182-69.818182s69.818182 30.254545 69.818181 69.818182v395.636364z"));
                }
                middle_frame.GoBack();
            }
        }

        private void Left_SideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e)
        {
            
       

        }

        //在打开页面时判断页面是否能返回，从而显示和隐藏按钮

        private void middle_frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (middle_frame.CanGoBack)
                Back_Button.Visibility = Visibility.Visible;
            else
                Back_Button.Visibility = Visibility.Hidden;
        }
        SideMenuItem Last_sideMenuItem;
        AnimationPath Last_animationPath;

        private void Change_Middle_Title(object sender, ExecutedRoutedEventArgs e)
        {
            middle_title.Text =((SideMenuItem)e.Parameter).Header.ToString();
            ((SideMenuItem)e.Parameter).Foreground = new SolidColorBrush(Color.FromRgb(251, 114, 153));
            ((AnimationPath)((SideMenuItem)e.Parameter).Icon).Stroke = new SolidColorBrush(Color.FromRgb(251, 114, 153));
            ((AnimationPath)((SideMenuItem)e.Parameter).Icon).IsPlaying = true;
                if (!string.Equals(Last_sideMenuItem.Header.ToString(), ((SideMenuItem)e.Parameter).Header.ToString()))
                {
                    Last_sideMenuItem.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Last_sideMenuItem.FontSize = 14;
                    Last_animationPath.IsPlaying = false;
                    Last_animationPath.Stroke= new SolidColorBrush(Color.FromRgb(0, 0, 0)); 

                }
            Last_sideMenuItem = (SideMenuItem)e.Parameter;
            Last_animationPath = ((AnimationPath)((SideMenuItem)e.Parameter).Icon);
        }

    }
}
