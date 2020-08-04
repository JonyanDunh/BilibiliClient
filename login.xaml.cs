using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ThoughtWorks.QRCode.Codec;

namespace Bilibili_Client
{

    public class Coding
    {

        public string MD5Encoding(string rawPass)
        {
            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder stb = new StringBuilder();
            foreach (byte b in hs)
            {
                // 以十六进制格式格式化
                stb.Append(b.ToString("x2"));
            }
            return stb.ToString();
        }

        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString()).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString()).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }


    }//加密函数

    public class Bilibili_Login//哔哩哔哩登录类
    {
        public DispatcherTimer Get_Scan_Login_Qrcode_status_Timer = new DispatcherTimer();
        public login Scan_login;
        public string Scan_oauthKey;

        //哔哩哔哩验证密钥类
        public class Verification_Key
        {
            public string gt;
            public string challenge;
            public string key;
            public string seccode;
            public string validate;
            public string tmp_code;
            public string phone;
            public int Sms_type = 0;
        }

        //哔哩哔哩Cookie类
        public class BiliCookie
        {
            public string DedeUserID;
            public string SESSDATA;
            public string bili_jct;
        }

        //获取结果的Cooie
        public BiliCookie Get_Cookie(string response)
        {
            BiliCookie biliCookie = new BiliCookie();
            biliCookie.DedeUserID = new Regex(@"DedeUserID=(?<DedeUserID>.*?)[&|;]", RegexOptions.IgnoreCase).Match(response).Groups["DedeUserID"].Value;
            biliCookie.SESSDATA = new Regex(@"SESSDATA=(?<SESSDATA>.*?)[&|;]", RegexOptions.IgnoreCase).Match(response).Groups["SESSDATA"].Value;
            biliCookie.bili_jct = new Regex(@"bili_jct=(?<bili_jct>.*?)[&|;]", RegexOptions.IgnoreCase).Match(response).Groups["bili_jct"].Value;
            return biliCookie;
        }

        //获取登录Hash
        public string Password_login_Get_Hash()
        {
            var client = new RestClient("http://passport.bilibili.com/login?act=getkey");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            client = null;
            request = null;
            response = null;
            return recommend["hash"].ToString();
        }

        //获取极验密钥
        public Verification_Key Get_Verification_Key(int plat)
        {
            Verification_Key verification_key = new Verification_Key();
            var client = new RestClient("http://passport.bilibili.com/web/captcha/combine?plat=" + plat);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            client = null;
            request = null;
            response = null;
            verification_key.gt = recommend["data"]["result"]["gt"].ToString();
            verification_key.challenge = recommend["data"]["result"]["challenge"].ToString();
            verification_key.key = recommend["data"]["result"]["key"].ToString();
            return verification_key;
        }

        //(网页接口)密码登录
        public void Password_login_Web(string username, string password, string hash, Verification_Key verification_key, login login)
        {
            Coding coding = new Coding();
            var rsa = new RSACryptoServiceProvider();
            var public_key = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDjb4V7EidX/ym28t2ybo0U6t0n
6p4ej8VjqKHg100va6jkNbNTrLQqMCQCAYtXMXXp2Fwkk6WR+12N9zknLjf+C9sx
/+l48mjUU8RqahiFD1XT/u2e0m2EN029OhCgkHx3Fc/KlFSIbak93EH/XlYis0w+
Xl69GV6klzgxW6d2xQIDAQAB";
            rsa.FromX509PublicKey(Convert.FromBase64String(public_key));
            var bytes = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes("mangohappy"), false));
            var client = new RestClient("http://passport.bilibili.com/web/login/v2");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("captchaType", "6");
            request.AddParameter("username", username);
            request.AddParameter("password", Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(hash + password), false)));
            request.AddParameter("keep", "true");
            request.AddParameter("key", verification_key.key);
            request.AddParameter("goUrl", coding.UrlEncode("https://www.bilibili.com/"));
            request.AddParameter("challenge", verification_key.challenge);
            request.AddParameter("validate", verification_key.validate);
            request.AddParameter("seccode", verification_key.seccode);
            IRestResponse response = client.Execute(request);

            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            string code = recommend["code"].ToString();
            if (string.Equals(code, "-2111"))
            {
                MessageBox.Show("由于您已开启二次验证，二次验证API出了点问题，请改用扫码或者验证码登录,抱歉了");
                /*Verification_Key Double_verification_key = Get_Verification_Key(7);
                Double_verification_key.Sms_type = 18;
                Double_verification_key.tmp_code = (recommend["data"].ToString()).Substring((recommend["data"].ToString()).IndexOf("tmp_token=") + 10, 32);*/
                login.Password_Login.Visibility = Visibility.Hidden;
                login.Sms_Code_Login.Visibility = Visibility.Visible;
                /*login.phone_textbox.Text = Double_verification_key.tmp_code;
                login.phone_textbox.IsReadOnly = true;
                login.Sms_code_Send_buttons.Content = "请验证";
                login.Sms_code_Send_buttons.IsEnabled = false;
                login.Sms_Login_button.Content = "请验证";
                login.Sms_Login_button.IsEnabled = false;
                login.sendKey_To_Geetest_page(Double_verification_key);*/

            }
            else
            {
                BiliCookie biliCookie = Get_Cookie(response.Content);
                Login_Success(biliCookie,login);
            }

            client = null;
            request = null;
        }

        //发送手机验证码
        public void Send_Sms(Verification_Key verification_key, login login)
        {
            if (verification_key.Sms_type == 18)//二次验证
            {
                var client = new RestClient("https://api.bilibili.com/x/safecenter/sms/send");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("application/x-www-form-urlencoded",
                    "type=18&captchaType=7" +
                    "&captcha_key=" + verification_key.key +
                    "&challenge=" + verification_key.challenge +
                    "&validate=" + verification_key.validate +
                    "&seccode=" + verification_key.seccode +
                    "&tmp_code=" + verification_key.tmp_code
                    , ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                MessageBox.Show(response.Content);
                JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
                string code = recommend["code"].ToString();
                if (!string.Equals(code, "0"))
                {
                    login.Sms_code_Send_buttons.Content = "失败";
                }
            }
            else if (verification_key.Sms_type == 21)//验证码登录
            {
                var client = new RestClient("http://passport.bilibili.com/web/sms/general/v2/send");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("application/x-www-form-urlencoded",
                    "tel=" + verification_key.phone +
                    "&cid=1&type=21&captchaType=6" +
                    "&key=" + verification_key.key +
                    "&challenge=" + verification_key.challenge +
                    "&validate=" + verification_key.validate +
                    "&seccode=" + verification_key.seccode, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
                string code = recommend["code"].ToString();
                if (!string.Equals(code, "0"))
                {
                    MessageBox.Show(recommend["message"].ToString());
                    login.Sms_code_Send_buttons.Content = "重发";

                }
                else if (string.Equals(code, "0"))
                {
                    login.Sms_code_Send_buttons.Content = "重发";
                    login.Sms_Login_button.IsEnabled = true;
                }

            }
        }

        //手机验证码登录
        public void Sms_login(string Sms_Code, string ID, login login)
        {
            var client = new RestClient("http://passport.bilibili.com/web/login/rapid");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Cookie", "sid=jd5w5fmn");
            request.AddParameter("cid", 1);
            request.AddParameter("tel", ID);
            request.AddParameter("smsCode", Sms_Code);
            IRestResponse response = client.Execute(request);
            BiliCookie biliCookie = Get_Cookie(response.Headers[4].ToString());
            Login_Success(biliCookie,login);
        }

        //获取登录二维码
        public void Get_Login_Qrcode(login login)
        {
            var client = new RestClient("http://passport.bilibili.com/qrcode/getLoginUrl");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            string url = recommend["data"]["url"].ToString();
            string oauthKey = recommend["data"]["oauthKey"].ToString();
            login.Qrcode_img.Source = login.NewQRCodeByThoughtWorks(url, ImageFormat.Png);
            Scan_login = login;
            Scan_oauthKey = oauthKey;
            Get_Scan_Login_Qrcode_status_Timer.Tick += new EventHandler(Get_Scan_Login_Qrcode_status);
            Get_Scan_Login_Qrcode_status_Timer.Interval = new TimeSpan(0, 0, 0, 1);
            Get_Scan_Login_Qrcode_status_Timer.Start();
        }

        //获取登录二维码的扫码状态
        public void Get_Scan_Login_Qrcode_status(object sender, EventArgs e)
        {

            var client = new RestClient("http://passport.bilibili.com/qrcode/getLoginInfo");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("oauthKey", Scan_oauthKey);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            if (string.Equals(recommend["status"].ToString(), "True"))
            {
                BiliCookie biliCookie = Get_Cookie(recommend["data"]["url"].ToString());
                Qrcode_Login(biliCookie, Scan_login);
                Get_Scan_Login_Qrcode_status_Timer.Stop();
            }
            else if (string.Equals(recommend["status"].ToString(), "False"))
            {
                string data = recommend["data"].ToString();
                if (string.Equals(data, "-5"))
                {
                    Scan_login.Scan_status.Content = "扫描成功,请在手机上确认是否授权";
                    Scan_login.Scan_status.Foreground= new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 211, 97));
                }
                else if (string.Equals(data, "-4"))
                {
                    Scan_login.Scan_status.Content = "请使用 哔哩哔哩客户端 扫码登录";
                    Scan_login.Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 114, 153));
                }
                else if (string.Equals(data, "-2"))
                {
                    Scan_login.Scan_status.Content = "二维码已失效,请点击二维码刷新";
                    Scan_login.Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(208, 2, 27));
                    Get_Scan_Login_Qrcode_status_Timer.Stop();
                }
            }
        }

        //二维码登录
        public void Qrcode_Login(BiliCookie biliCookie,login login)
        {
            Login_Success(biliCookie,login);

        }

        //登录成功后的操作
        public void Login_Success(BiliCookie biliCookie, login login)
        {
            if (false == Directory.Exists(@"Data\User\Account\Cookie"))
            {
                Directory.CreateDirectory(@"Data\User\Account\Cookie");
            }
            File.WriteAllText(@"Data\User\Account\Cookie\Login_User.Json", JsonConvert.SerializeObject(biliCookie), Encoding.UTF8);
            login.Login_Success();

        }

    }

    public partial class login : Page
    {
        public delegate void Login_SendMessage_To_Mainwindow();
        public Login_SendMessage_To_Mainwindow login_sendMessage_To_Mainwindow;
        public Login_SendMessage_To_Mainwindow login_open_geetest_page;
        public Login_SendMessage_To_Mainwindow Login_Success;

        public delegate void SendKey_To_Geetest_page(Bilibili_Login.Verification_Key verification_key);
        public SendKey_To_Geetest_page sendKey_To_Geetest_page;

        Bilibili_Login bilibili = new Bilibili_Login();
        public login()
        {
            InitializeComponent();

        }

        //从主窗口接收信息
        public void Login_Recevie_From_Mainwindow()
        {

        }

        //从验证页面接收信息
        public void Login_Recevie_Key_From_Geetest_page(Bilibili_Login.Verification_Key verification_key)
        {

            bilibili.Password_login_Web(account_textbox.Text, password_textbox.Text, bilibili.Password_login_Get_Hash(), verification_key, this);
        }

        //从验证页面接收Key
        public void Login_Recevie_SmsKey_From_Geetest_page(Bilibili_Login.Verification_Key verification_key)
        {
            bilibili.Send_Sms(verification_key, this);
        }

        //密码登录按钮
        private void Password_Login_buttons_Click(object sender, RoutedEventArgs e)
        {
            login_open_geetest_page();
            sendKey_To_Geetest_page(bilibili.Get_Verification_Key(6));
        }

        //验证码登录发送验证码
        private void Sms_code_Send(object sender, RoutedEventArgs e)
        {
            login_open_geetest_page();
            Bilibili_Login.Verification_Key verification_key = bilibili.Get_Verification_Key(6);
            verification_key.Sms_type = 21;

            verification_key.phone = phone_textbox.Text;
            sendKey_To_Geetest_page(verification_key);

        }

        //验证码登录按钮
        private void Sms_code_Login(object sender, RoutedEventArgs e)
        {
            bilibili.Sms_login(smscode_textbox.Text, phone_textbox.Text,this);
        }

        //点击二维码刷新二维码
        private void Refresh_Qrcode(object sender, MouseButtonEventArgs e)
        {
            bilibili.Get_Scan_Login_Qrcode_status_Timer.Stop();
            bilibili.Get_Login_Qrcode(this);
            Scan_status.Content = "请使用 哔哩哔哩客户端 扫码登录";
            Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 114, 153));
        }

        //切换到密码登录页面
        private void Password(object sender, MouseButtonEventArgs e)
        {
            Password_Login.Visibility = Visibility.Visible;
            Sms_Code_Login.Visibility = Visibility.Hidden;
            QrCode_Login.Visibility = Visibility.Hidden;
        }

        //切换到验证码登录页面
        private void Sms(object sender, MouseButtonEventArgs e)
        {
            Password_Login.Visibility = Visibility.Hidden;
            Sms_Code_Login.Visibility = Visibility.Visible;
            QrCode_Login.Visibility = Visibility.Hidden;
        }

        //切换到二维码登录页面
        private void Qrcode(object sender, MouseButtonEventArgs e)
        {
            Password_Login.Visibility = Visibility.Hidden;
            Sms_Code_Login.Visibility = Visibility.Hidden;
            QrCode_Login.Visibility = Visibility.Visible;
            bilibili.Get_Login_Qrcode(this);
        }

        //生成二维码函数
        public BitmapImage NewQRCodeByThoughtWorks(string codeContent, ImageFormat imgType)
        {
            QRCodeEncoder encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//编码方式(注意：BYTE能支持中文，ALPHA_NUMERIC扫描出来的都是数字)
            encoder.QRCodeScale = 10;//大小(值越大生成的二维码图片像素越高)
            encoder.QRCodeVersion = 0;//版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//错误效验、错误更正(有4个等级)
            encoder.QRCodeBackgroundColor = System.Drawing.Color.White;
            encoder.QRCodeForegroundColor = System.Drawing.Color.FromArgb(251, 114, 153);

            Bitmap bcodeBitmap = encoder.Encode(codeContent);
            BitmapImage bitmapImage = BitmapToBitmapImage(bcodeBitmap);
            bcodeBitmap.Dispose();
            return bitmapImage;
        }

        //Bitmap转BitmapImage
        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bitmapSource = new Bitmap(bitmap.Width, bitmap.Height);
            int i, j;
            for (i = 0; i < bitmap.Width; i++)
                for (j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color pixelColor = bitmap.GetPixel(i, j);
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);
                    bitmapSource.SetPixel(i, j, newColor);
                }
            MemoryStream ms = new MemoryStream();
            bitmapSource.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }

}
