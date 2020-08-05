using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            bs = null;
            hs = null;
            md5 = null;
            md5.Dispose();
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

            bilibili.Password_login_Web(account_textbox.Text, password_textbox.Password, bilibili.Password_login_Get_Hash(), verification_key, this);
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
            bilibili.Sms_login(smscode_textbox.Password, phone_textbox.Text,this);
        }

        //点击二维码刷新二维码
        private void Refresh_Qrcode(object sender, MouseButtonEventArgs e)
        {
            bilibili.Get_Scan_Login_Qrcode_status_Timer.Stop();
            bilibili.Get_Login_Qrcode(this);
            Scan_status.Text = "请使用 哔哩哔哩客户端 扫码登录";
            Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 114, 153));
        }

        //切换到密码登录页面
        private void Password(object sender, MouseButtonEventArgs e)
        {
            if (QrCode_Login.Visibility == Visibility.Visible)
                bilibili.Get_Scan_Login_Qrcode_status_Timer.Stop();
            Password_Login.Visibility = Visibility.Visible;
            Sms_Code_Login.Visibility = Visibility.Hidden;
            QrCode_Login.Visibility = Visibility.Hidden;

        }

        //切换到验证码登录页面
        private void Sms(object sender, MouseButtonEventArgs e)
        {
            if (QrCode_Login.Visibility == Visibility.Visible)
                bilibili.Get_Scan_Login_Qrcode_status_Timer.Stop();
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
            encoder = null;
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

            bitmapSource.Dispose();
            ms.Dispose();
            bitmap.Dispose();
            

            return bitmapImage;
        }

    }

}
