using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bilibili_Client
{
    /// <summary>
    /// login.xaml 的交互逻辑
    /// </summary>
    /// 

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


    }

    public class Bilibili
    {

        public class Verification_Key
        {
            public string gt;
            public string challenge;
            public string key;
            public string seccode;
            public string validate;
            public string Id;//Id为手机号或tmp_code
            public int Sms_type = 0;
        }
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
        public Verification_Key Get_Verification_Key(int plat)
        {
            Verification_Key verification_key = new Verification_Key();
            var client = new RestClient("http://passport.bilibili.com/web/captcha/combine?plat="+plat);
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
        public void Password_login_Web(string username, string password, string hash, Verification_Key verification_key,login login)
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
            if (string.Equals(code,"-2111"))
            {
                MessageBox.Show("您已开启二次验证，请验证");
                Verification_Key Double_verification_key = Get_Verification_Key(7);
                Double_verification_key.Sms_type = 18;
                Double_verification_key.Id = (recommend["data"].ToString()).Substring((recommend["data"].ToString()).IndexOf("tmp_token=")+10,32);
                login.Password_Login.Visibility = Visibility.Hidden;
                login.Sms_Code_Login.Visibility = Visibility.Visible;
                login.phone_textbox.Text = Double_verification_key.Id;
                login.phone_textbox.IsReadOnly = true;
                login.Sms_code_Send_buttons.Content = "请验证";
                login.Sms_code_Send_buttons.IsEnabled = false;
                login.Sms_Login_button.Content = "请验证";
                login.Sms_Login_button.IsEnabled = false;
                login.sendKey_To_Geetest_page(Double_verification_key);

            }else
                MessageBox.Show(response.Content);

            client = null;
            request = null;
        }
        public void Send_Sms(Verification_Key verification_key, login login)//Id为手机号或tmp_code
        {
            if (verification_key.Sms_type == 18)//二次验证
            {
                var client = new RestClient("https://api.bilibili.com/x/safecenter/sms/send");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("type", "18");
                request.AddParameter("captcha_type", "7");
                request.AddParameter("captcha_key", verification_key.key);
                request.AddParameter("captcha", "");
                request.AddParameter("challenge", verification_key.challenge);
                request.AddParameter("seccode", verification_key.seccode);
                request.AddParameter("validate", verification_key.validate);
                request.AddParameter("tmp_code", verification_key.Id);
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


            }
        }
        public void Sms_login(string Sms_Code)
        {
            
        }
    }

    public partial class login : Page
    {
        public delegate void Login_SendMessage_To_Mainwindow();
        public Login_SendMessage_To_Mainwindow login_sendMessage_To_Mainwindow;
        public Login_SendMessage_To_Mainwindow login_open_geetest_page;

        public delegate void SendKey_To_Geetest_page(Bilibili.Verification_Key verification_key);
        public SendKey_To_Geetest_page sendKey_To_Geetest_page;

        Bilibili bilibili = new Bilibili();
        public login()
        {
            InitializeComponent();

        }
        public void Login_Recevie_From_Mainwindow()//从主窗口接收信息
        {

        }


        bool IsLogin=false;

        public void Login_Recevie_Key_From_Geetest_page(Bilibili.Verification_Key verification_key)//从验证页面接收信息
        {

            bilibili.Password_login_Web(account_textbox.Text, password_textbox.Text, bilibili.Password_login_Get_Hash(), verification_key,this);
        }
        public void Login_Recevie_SmsKey_From_Geetest_page(Bilibili.Verification_Key verification_key)//从验证页面接收信息
        {
            bilibili.Send_Sms(verification_key,this);
        }

        private void Password_Login_buttons_Click(object sender, RoutedEventArgs e)
        {
            login_open_geetest_page();
            sendKey_To_Geetest_page(bilibili.Get_Verification_Key(6));
        }

        private void Qrcode(object sender, MouseButtonEventArgs e)
        {
        }


        private void Sms_code_Send(object sender, RoutedEventArgs e)
        {
            login_open_geetest_page();
            Bilibili.Verification_Key verification_key = bilibili.Get_Verification_Key(6);
            verification_key.Sms_type = 21;
            verification_key.Id = phone_textbox.Text;
            sendKey_To_Geetest_page(verification_key);

        }

        private void Sms_code_Login(object sender, RoutedEventArgs e)
        {

        }
    }

}
