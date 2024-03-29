﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace Bilibili_Client
{
    public class Bilibili_Login//哔哩哔哩登录类

    {
        public DispatcherTimer Get_Scan_Login_Qrcode_status_Timer;
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
            verification_key.gt = recommend["data"]["result"]["gt"].ToString();
            verification_key.challenge = recommend["data"]["result"]["challenge"].ToString();
            verification_key.key = recommend["data"]["result"]["key"].ToString();
            client = null;
            request = null;
            response = null;
            recommend = null;
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
            else if (string.Equals(code, "0"))
            {
                BiliCookie biliCookie = Get_Cookie(response.Content);
                Login_Success(biliCookie, login);

            }
            else
            {
                MessageBox.Show(response.Content);
            }
            client = null;
            request = null;
            response = null;
            recommend = null;
            code = null;
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
                client = null;
                request = null;
                response = null;
                recommend = null;
                code = null;
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
                    MessageBox.Show(response.Content);
                    login.Sms_code_Send_buttons.Content = "发送";
                }
                else if (string.Equals(code, "0"))
                {
                    login.Sms_code_Send_buttons.Content = "重发";
                    login.Sms_Login_button.IsEnabled = true;

                }
                client = null;
                request = null;
                response = null;
                recommend = null;
                code = null;

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
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            string code = recommend["code"].ToString();
            if (!string.Equals(code, "0"))
            {
                MessageBox.Show(response.Content);
            }
            else if (string.Equals(code, "0"))
            {
                BiliCookie biliCookie = Get_Cookie(response.Headers[4].ToString());
                Login_Success(biliCookie, login);

            }        
            client = null;
            request = null;
            response = null;
            recommend = null;
            code = null; 
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
            Get_Scan_Login_Qrcode_status_Timer = new DispatcherTimer();
            Get_Scan_Login_Qrcode_status_Timer.Tick += new EventHandler(Get_Scan_Login_Qrcode_status);
            Get_Scan_Login_Qrcode_status_Timer.Interval = new TimeSpan(0, 0, 0, 1);
            Get_Scan_Login_Qrcode_status_Timer.Start();
            url = null;
            oauthKey = null;
            client = null;
            request = null;
            response = null;
            recommend = null;
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
                Get_Scan_Login_Qrcode_status_Timer = null;
            }
            else if (string.Equals(recommend["status"].ToString(), "False"))
            {
                string data = recommend["data"].ToString();
                if (string.Equals(data, "-5"))
                {
                    Scan_login.Scan_status.Text = "扫描成功,请在手机上确认是否授权";
                    Scan_login.Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 211, 97));
                }
                else if (string.Equals(data, "-4"))
                {
                    Scan_login.Scan_status.Text = "请使用 哔哩哔哩客户端 扫码登录";
                    Scan_login.Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(251, 114, 153));
                }
                else if (string.Equals(data, "-2"))
                {
                    Scan_login.Scan_status.Text = "二维码已失效,请点击二维码刷新";
                    Scan_login.Scan_status.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(208, 2, 27));
                    Get_Scan_Login_Qrcode_status_Timer.Stop();
                    Get_Scan_Login_Qrcode_status_Timer =null;

                }
            }
            client = null;
            request = null;
            response = null;
            recommend = null;
        }

        //二维码登录
        public void Qrcode_Login(BiliCookie biliCookie, login login)
        {
            Login_Success(biliCookie, login);

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

    public class Bilibili_Account//哔哩哔哩账户类
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
                {
                    client = null;
                    request = null;
                    response = null;
                    return true; 
                }
                else
                {
                    client = null;
                    request = null;
                    response = null;
                    return false; 
                }
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
            string User_Name = data["uname"].ToString();
            mainWindow.Login_Button.Visibility = Visibility.Hidden;
            mainWindow.User_Name_Grid.Visibility = Visibility.Visible;
            mainWindow.User_SideMenu.Visibility = Visibility.Visible;
            mainWindow.User_Name_Label.Text = User_Name;
            mainWindow.User_Cover_Img.Source = new BitmapImage(new Uri(User_Cover, UriKind.RelativeOrAbsolute));
            mainWindow.User_Exp_Bar.Value = (data["level_info"]["current_exp"].ToObject<double>() / data["level_info"]["current_min"].ToObject<double>()) * 100;
            client = null;
            request = null;
            response = null;
            User_Name = null;
            User_Cover = null;
            data = null;
            recommend = null;
        }
        public string Get_User_Data(string str1, string str2, bool If_Doubel, MainWindow mainWindow)
        {
            string SESSDATA = mainWindow.BiliCookie.SESSDATA;
            if (If_Doubel)
            {
                var client = new RestClient("http://api.bilibili.com/x/web-interface/nav");
                var request = new RestRequest(Method.GET);
                request.AddCookie("SESSDATA", SESSDATA);
                IRestResponse response = client.Execute(request);
                client = null;
                request = null;
                return ((JObject)JsonConvert.DeserializeObject(response.Content))["data"][str1][str2].ToString();
            }
            else
            {
                var client = new RestClient("http://api.bilibili.com/x/web-interface/nav");
                var request = new RestRequest(Method.GET);
                request.AddCookie("SESSDATA", SESSDATA);
                IRestResponse response = client.Execute(request);
                client = null;
                request = null;
                return ((JObject)JsonConvert.DeserializeObject(response.Content))["data"][str1].ToString();
            }



        }


    }

    public class Bilibili_Video
    {
        public struct video_info
        {
            public bool Success;
            public string aid;//视频avID	
            public string bvid;//视频bvID	
            public int videos;//视频分P总数	
            public int tid;//分区ID	
            public string tname;//子分区名称	
            public int copyright;//版权标志	1：自制 2：转载
            public string pic;//视频封面图片url	
            public string title;//视频标题	
            public string ctime;//视频审核通过时间	
            public string desc;//视频简介	
            public string mid;//UP主UID	
            public string name;//UP主昵称	
            public string face;//UP主头像	
            public int view;//观看次数 普通：0 屏蔽时：-1
            public int danmaku;//弹幕数	
            public int reply;//评论数	
            public int favorite;//收藏数	
            public int coin;//投币数	
            public int share;//分享数	
            public int like;//获赞数	
            public pages[] pages;
        }
        public struct pages//视频分P列表
        {
            public string cid;//当前分P CID	
            public int page;//当前分P	
            public string part;//当前分P标题	
        }
        public struct P_data
        {
            public bool Success;
            public string url;
            public string[] accept_description;
            public int[] accept_quality;
        }
        public video_info gets_video_info(string aid)
        {
            var client = new RestClient("http://api.bilibili.com/x/web-interface/view?aid=" + aid);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            video_info videoinfo = new video_info();
            JToken data = recommend["data"];
            if (Convert.ToInt32(recommend["code"]) == 0)
            {
                videoinfo.Success = true;
                videoinfo.aid = aid;
                videoinfo.bvid = data["bvid"].ToString();
                videoinfo.videos = Convert.ToInt32(data["videos"]);
                videoinfo.tid= Convert.ToInt32(data["tid"]);
                videoinfo.tname = data["tname"].ToString();
                videoinfo.copyright = Convert.ToInt32(data["copyright"]);
                videoinfo.pic = data["pic"].ToString();
                videoinfo.title = data["title"].ToString();
                videoinfo.ctime = data["ctime"].ToString();
                videoinfo.desc = data["desc"].ToString();
                videoinfo.mid= data["owner"]["mid"].ToString();
                videoinfo.name = data["owner"]["name"].ToString();
                videoinfo.face = data["owner"]["face"].ToString();
                videoinfo.view = Convert.ToInt32(data["stat"]["view"]);
                videoinfo.danmaku = Convert.ToInt32(data["stat"]["danmaku"]);
                videoinfo.reply = Convert.ToInt32(data["stat"]["reply"]);
                videoinfo.favorite = Convert.ToInt32(data["stat"]["favorite"]);
                videoinfo.coin = Convert.ToInt32(data["stat"]["coin"]);
                videoinfo.share = Convert.ToInt32(data["stat"]["share"]);
                videoinfo.like = Convert.ToInt32(data["stat"]["like"]);
                videoinfo.pages = new pages[videoinfo.videos];
                 for(int i=0;i<videoinfo.videos;i++)
                 {
                     videoinfo.pages[i].cid = data["pages"][i]["cid"].ToString();
                     videoinfo.pages[i].page = Convert.ToInt32(data["pages"][i]["page"]);
                     videoinfo.pages[i].part = data["pages"][i]["part"].ToString();
                }
                client = null;
                request = null;
                response = null;
                return videoinfo;
            }else
            {

                MessageBox.Show("该视频不存在");
                videoinfo.Success = false;
                client = null;
                request = null;
                response = null;
                return videoinfo;

            }

        }
        public P_data Get_Video_Stream(string avid,string cid,int clarity,bool Islogin,Bilibili_Account.BiliCookie biliCookie)
        {
            P_data p_Data = new P_data();
            var client = new RestClient("http://api.bilibili.com/x/player/playurl?cid="+cid+"&avid="+avid+"&qn="+clarity);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            if(Islogin)
            request.AddCookie("SESSDATA", biliCookie.SESSDATA);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            if ((int)recommend["code"] == 0)
            {
                p_Data.Success = true;
                JToken data = recommend["data"];
                p_Data.url = data["durl"][0]["url"].ToString();
                p_Data.accept_description = new string[data["accept_description"].Count()];
                p_Data.accept_quality = new int[data["accept_description"].Count()];
                for (int i=0;i< data["accept_description"].Count();i++)
                {
                    p_Data.accept_description[i] = data["accept_description"][i].ToString();
                    p_Data.accept_quality[i] = (int)data["accept_quality"][i];

                }
                 
                client = null;
                request = null;
                response = null;
                
                return p_Data;
            }else
            {
                p_Data.Success = false;
                MessageBox.Show("视频加载失败");
                client = null;
                request = null;
                response = null;
                return p_Data;
            }

        }




    }
}
