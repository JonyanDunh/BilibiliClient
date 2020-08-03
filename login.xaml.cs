using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
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
        public string DecryptPublicKeyJava(string publicKeyPem, string data, string encoding = "UTF-8")
        {
            string publicKeyJava = File.ReadAllText(publicKeyPem);
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKeyJava));
            Console.WriteLine(publicKeyParam);
            byte[] cipherbytes = Convert.FromBase64String(data);
            RsaEngine rsa = new RsaEngine();
            rsa.Init(false, publicKeyParam);//参数true表示加密/false表示解密。
            cipherbytes = rsa.ProcessBlock(cipherbytes, 0, cipherbytes.Length);
            return Encoding.GetEncoding(encoding).GetString(cipherbytes);
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
            StreamWriter sw = File.CreateText(@"Data\PublicKey.pem");
            sw.Write(recommend["key"]);
            sw.Flush();
            sw.Close();
            return recommend["hash"].ToString();
        }
        public Verification_Key Password_login_Get_Verification_Key()
        {
            Verification_Key verification_key = new Verification_Key();
            var client = new RestClient("http://passport.bilibili.com/web/captcha/combine?plat=6");
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
        public void Password_login_Web(string username, string password, string hash, Verification_Key verification_key)
        {
            Coding coding = new Coding();
            var client = new RestClient("http://passport.bilibili.com/web/login/v2");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("captchaType", "6");
            request.AddParameter("username", username);
            request.AddParameter("password", coding.DecryptPublicKeyJava(@"Data\PublicKey.pem", password));
            request.AddParameter("keep", "true");
            request.AddParameter("key", verification_key.key);
            request.AddParameter("goUrl", coding.UrlEncode("https://www.bilibili.com/"));
            request.AddParameter("challenge", verification_key.challenge);
            request.AddParameter("validate", verification_key.validate);
            request.AddParameter("seccode", verification_key.seccode);
            IRestResponse response = client.Execute(request);
           // MessageBox.Show(response.Content);
            
            client = null;
            request = null;
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
        

        
       
        public void Login_Recevie_Key_From_Geetest_page(Bilibili.Verification_Key verification_key)//从验证页面接收信息
        {
             
        bilibili.Password_login_Web(account_textbox.Text, password_textbox.Text, bilibili.Password_login_Get_Hash(), verification_key);
        }

        private void Login_buttons_Click(object sender, RoutedEventArgs e)
        {
            login_open_geetest_page();
            sendKey_To_Geetest_page(bilibili.Password_login_Get_Verification_Key()); 
        }

        private void Qrcode(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
