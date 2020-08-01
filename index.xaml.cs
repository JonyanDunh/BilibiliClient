using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Bilibili_Client
{
    /// <summary>
    /// index.xaml 的交互逻辑
    /// </summary>
    
    public partial class index : Page
    {
        ManualResetEvent Thread_blocking = new ManualResetEvent(false);//线程阻塞
        class up_info
        {
            public String head_img;   // up头像
        }
        class video_info
        {
            public String head_img;   // up头像
            public String introduction;   // 介绍
            public String comments;   // 评论
            public String release_time;   // 发布时间
        }
        public index()
        {
            InitializeComponent();
            Thread thread = new Thread(Home_Recommendation);//加载推荐视频的函数加入一个新的子线程
            thread.Start();//线程开始
            if (false == Directory.Exists(@"Data\Cache\Img\Avatar\"))
            {
                //创建DATA文件夹
                Directory.CreateDirectory(@"Data\Cache\Img\Avatar\");
            }
            if (false == Directory.Exists(@"Data\Cache\Img\Cover\"))
            {
                //创建DATA文件夹
                Directory.CreateDirectory(@"Data\Cache\Img\Cover\");
            }


        }
        private void Home_Recommendation()
        {
            while(true)
            {
                Thread_blocking.WaitOne();
                var client = new RestClient("https://app.bilibili.com/x/v2/feed/index?idx=1596246654&flush=0&column=4&device=pad&pull=false&build=5520400&mobi_app=iphone&platform=ios&ts=1596246653");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            Newtonsoft.Json.Linq.JToken items = recommend["data"]["items"];
                for (int i = 1; i <= items.Count()-1; i++)
            {
                new Thread((obj) =>
                {
                    WebClient myWebClient = new WebClient();
                    string avatar_path = System.Environment.CurrentDirectory + @"\Data\Cache\Img\Avatar\" + items[(int)obj]["args"]["up_id"].ToString() + ".png";
                    string cover_path = System.Environment.CurrentDirectory + @"\Data\Cache\Img\Cover\" + items[(int)obj]["args"]["aid"].ToString() + ".png";
                    if (!System.IO.File.Exists(avatar_path))
                    {
                        myWebClient.DownloadFile(items[(int)obj]["avatar"]["cover"].ToString(), avatar_path);
                    }
                    if (!System.IO.File.Exists(cover_path))
                    {
                        myWebClient.DownloadFile(items[(int)obj]["cover"].ToString(), cover_path);
                    }
                    List<double_row_video> double_row_video = new List<double_row_video>
                    {
                        new double_row_video(
                  avatar_path,//up头像
                  items[(int)obj]["desc"].ToString(),//up名字
                  cover_path,//封面
                  items[(int)obj]["cover_left_text_1"].ToString(),//时长
                  items[(int)obj]["title"].ToString().Length>17?items[(int)obj]["title"].ToString().Substring(0,17)+"...":items[(int)obj]["title"].ToString(),//标题
                  items[(int)obj]["args"]["rname"].ToString(),//分区
                  items[(int)obj]["cover_left_text_2"].ToString(),//播放量
                  items[(int)obj]["cover_left_text_3"].ToString()//弹幕数
                  )
                       };
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        
                        content_box.Items.Add(double_row_video);//按照模板加入一个item
                        
                    });
                   

                }).Start(i);
               
            } 
                Thread_blocking.Reset();
               
            }

        }
        public class double_row_video
        {
            public string head_img_url { get; private set; }
            public string video_up { get; private set; }
            public string video_cover { get; private set; }
            public string video_title { get; private set; }
            public string video_play_volume { get; private set; }
            public string video_barrages { get; private set; }
            public string video_duration { get; private set; }
            public string video_partition { get; private set; }
            public List<tag> tag_control { set; get; }


            public double_row_video(
                string up_head, //up头像
                string up,//up名字
                string cover,//封面
                string duration,//时长
                string title,//标题
                string partition,//分区
                string play_volume, //播放量
                string barrages //弹幕数
                                //List<tag> video_tags//视频标签
                )
            {
                head_img_url = up_head;//up头像
                video_up = up;//up名字

                video_cover = cover;//封面
                video_duration = duration;//时长
                video_title = title;//标题
                video_partition = partition;//分区
                video_play_volume = play_volume;//播放量
                video_barrages = barrages;//弹幕数
                //tag_control = video_tags;//视频标签

            }
        }
        public class tag
        {
            public string video_tags { get; private set; }
            public tag(string tag_name)
            {
                video_tags = tag_name;
            }
        }
        static up_info gets_up_info(string uid)
        {
            var client = new RestClient("http://api.bilibili.com/x/space/acc/info?mid=" + uid);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            up_info upinfo = new up_info();
            upinfo.head_img = recommend["data"]["face"].ToString();

            return upinfo;

        }
        static video_info gets_video_info(string aid)
        {
            var client = new RestClient("http://api.bilibili.com/x/web-interface/view?aid=" + aid);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            video_info videoinfo = new video_info();
            videoinfo.head_img = recommend["data"]["owner"]["face"].ToString();
            videoinfo.introduction = recommend["data"]["desc"].ToString();
            videoinfo.comments = recommend["data"]["stat"]["reply"].ToString();
            videoinfo.release_time = recommend["data"]["pubdate"].ToString();
            return videoinfo;

        }
        public static String GetTime(string timeStamp)
        {
            if (timeStamp.Length > 10)
            {
                timeStamp = timeStamp.Substring(0, 10);
            }
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);

            return string.Equals(dateTimeStart.Add(toNow).ToString("YY-MM-dd"), DateTime.Now.ToString("YY-MM-dd")) ? dateTimeStart.Add(toNow).ToString("HH:mm") : dateTimeStart.Add(toNow).ToString("MM-dd");
        }
        public static List<tag> Get_video_tags(string aid)
        {
            var client = new RestClient("http://api.bilibili.com/x/tag/archive/tags?aid=" + aid);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            video_info videoinfo = new video_info();
            List<tag> video_tag = new List<tag>();
            Newtonsoft.Json.Linq.JToken items = recommend["data"];
            for (int i = 0; i < (items.Count() >= 6 ? 6 : items.Count()); i++)
            {

                video_tag.Add(new tag(items[i]["tag_name"].ToString()));

            }
            return video_tag;
        }


        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (index_scrollViewer.ScrollableHeight == index_scrollViewer.ContentVerticalOffset)
                Thread_blocking.Set();
        }
    }
}
