using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;



namespace Bilibili_Client
{
    /// <summary>
    /// index.xaml 的交互逻辑
    /// </summary>

    public partial class index : Page
    {
        ManualResetEvent Thread_blocking = new ManualResetEvent(true);//线程阻塞
        public index()
        {
            InitializeComponent();
            Thread thread = new Thread(Home_Recommendation);//加载推荐视频的函数加入一个新的子线程
            thread.Start();//线程开始
        }
        private void Home_Recommendation()
        {
            while(true)
            {
                
                Thread_blocking.WaitOne();
            var client = new RestClient("https://app.bilibili.com/x/v2/feed/index?device=pad&mobi_app=iphone");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
                JToken items = recommend["data"]["items"];
                for (int i = 1; i < items.Count()-1; i++)
            {
                    new Thread((obj) =>
                    {
                        List<double_row_video> double_row_video = new List<double_row_video>
                        {
                            new double_row_video(
                      items[(int)obj]["avatar"]["cover"].ToString()+"@22w_22h_1c_95q",//up头像
                      items[(int)obj]["desc"].ToString(),//up名字
                      items[(int)obj]["cover"].ToString()+"@320w_200h_1c_95q",//封面
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
                            double_row_video = null;
                        });
                    }).Start(i);
                }
                client = null;
                request = null;
                response = null;
                recommend = null;
                Thread_blocking.Reset();
            }

        }

        public class double_row_video
        {
            public string head_img_url { get; private set; }
            public string video_cover { get; private set; }
            public string video_up { get; private set; }
            public string video_title { get; private set; }
            public string video_play_volume { get; private set; }
            public string video_barrages { get; private set; }
            public string video_duration { get; private set; }
            public string video_partition { get; private set; }


            public double_row_video(
                string up_head, //up头像
                string up,//up名字
                string cover,//封面
                string duration,//时长
                string title,//标题
                string partition,//分区
                string play_volume, //播放量
                string barrages //弹幕数
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

            }
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (index_scrollViewer.ScrollableHeight == index_scrollViewer.ContentVerticalOffset&& content_box.Items.Count>=18)
                Thread_blocking.Set();
        }


    }
}
