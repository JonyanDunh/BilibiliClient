using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Bilibili_Client
{
    
    public class Bilibili
    {
        public class BiliCookie
        {
            public string DedeUserID;
            public string SESSDATA;
            public string bili_jct;
        }
        public bool Check_Login_status()
        {
            if (File.Exists(@"Data\User\Account\Cookie\Login_User.Json"))//如果存在用户数据文件
            {
                string Login_User_Json = File.ReadAllText(@"Data\User\Account\Cookie\Login_User.Json");
                var client = new RestClient("http://api.bilibili.com/x/web-interface/nav");
                var request = new RestRequest(Method.GET);
                request.AddCookie("SESSDATA", ((JObject)JsonConvert.DeserializeObject(Login_User_Json))["SESSDATA"].ToString());
                IRestResponse response = client.Execute(request);
                if (string.Equals(((JObject)JsonConvert.DeserializeObject(response.Content))["code"].ToString(), "0"))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }

        }
        public void Set_User_Data(MainWindow mainWindow)
        {
            string Login_User_Json = File.ReadAllText(@"Data\User\Account\Cookie\Login_User.Json");
            mainWindow.BiliCookie.SESSDATA = ((JObject)JsonConvert.DeserializeObject(Login_User_Json))["SESSDATA"].ToString();
            mainWindow.BiliCookie.DedeUserID = ((JObject)JsonConvert.DeserializeObject(Login_User_Json))["DedeUserID"].ToString();
            mainWindow.BiliCookie.bili_jct = ((JObject)JsonConvert.DeserializeObject(Login_User_Json))["bili_jct"].ToString();
            var client = new RestClient("http://api.bilibili.com/x/web-interface/nav");
            var request = new RestRequest(Method.GET);
            request.AddCookie("SESSDATA", mainWindow.BiliCookie.SESSDATA);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            JToken data = recommend["data"];
            string User_Cover = data["face"].ToString();
            string User_Name= data["uname"].ToString();
            mainWindow.User_Name_Label.Content = User_Name;
            mainWindow.User_Cover_Img.Source = new BitmapImage(new Uri(User_Cover, UriKind.RelativeOrAbsolute)); 
        }
        public string Get_User_Data(string str1,string str2,bool If_Doubel, MainWindow mainWindow)
        {
            string SESSDATA = mainWindow.BiliCookie.SESSDATA;
            if (If_Doubel)
            {
                var client = new RestClient("http://api.bilibili.com/x/web-interface/nav");
                var request = new RestRequest(Method.GET);
                request.AddCookie("SESSDATA", SESSDATA);
                IRestResponse response = client.Execute(request);
                return ((JObject)JsonConvert.DeserializeObject(response.Content))["data"][str1][str2].ToString();
            }
            else
            {
                var client = new RestClient("http://api.bilibili.com/x/web-interface/nav");
                var request = new RestRequest(Method.GET);
                request.AddCookie("SESSDATA", SESSDATA);
                IRestResponse response = client.Execute(request);
                return ((JObject)JsonConvert.DeserializeObject(response.Content))["data"][str1].ToString();
            }



        }


    }
    public partial class MainWindow : Window
    {

        // 实例化计时器
        private DispatcherTimer GCTimer = new DispatcherTimer();
        index index_page = new index();
        login login_page = new login();
        Space space_page = new Space();
        geetest geetest_page = new geetest();
        Bilibili bilibili = new Bilibili();
        public Bilibili.BiliCookie BiliCookie = new Bilibili.BiliCookie();
        public bool IsLogin = false;
        private delegate void MainWindow_SendMessage_To_Login_page();//主窗口发送给登录窗口类
        private MainWindow_SendMessage_To_Login_page mainWindow_SendMessage_To_Login_page;
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
            Index_Button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 114, 153));//主页的按钮设置为粉色
            if (bilibili.Check_Login_status())//判断是否登录
            {
                IsLogin = true;
                bilibili.Set_User_Data(this);
            }
        }

        //屏幕自适应
        private void Adaptive()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;//显示位置屏幕居中
            mainwindow.Width = (SystemParameters.PrimaryScreenWidth) * 0.83;
            mainwindow.Height = (SystemParameters.PrimaryScreenHeight) * 0.83;
            left_grid.Width = mainwindow.Width * 0.145;
            right_grid.Width = mainwindow.Width * 0.234;
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
        }

        //创建文件夹
        private void Create_data()
        {
            if (false == Directory.Exists(@"Data\Cache\Img"))
            {
                Directory.CreateDirectory(@"Data\Cache\Img");
            }

        }
        //主窗口从登录页面传回来的数据
        private void MainWindow_Recevie_From_Login_Page()
        {


        }
        private void Login_Success()
        {
            IsLogin = true;
            bilibili.Set_User_Data(this);
            middle_frame.Navigate(space_page);
            middle_title.Content = bilibili.Get_User_Data("uname", "", false, this) + "的个人空间";

        }
        //登录页面打开极验页面
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

        //打开个人主页的按钮
        private void Open_User_Space(object sender, MouseButtonEventArgs e)
        {
            if (IsLogin)
            {
                middle_frame.Navigate(space_page);
                middle_title.Content = bilibili.Get_User_Data("uname", "", false, this)+"的个人空间";
               

            }
            else
            {
                middle_frame.Navigate(login_page);
            }
        }
        //打开首页的按钮
        private void Open_Index(object sender, RoutedEventArgs e)
        {
            middle_frame.Navigate(index_page);
            Index_Button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 114, 153));
            middle_title.Content = "首页";
        }
    }
}
