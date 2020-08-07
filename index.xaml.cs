using HandyControl.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;



namespace Bilibili_Client
{
    /// <summary>
    /// index.xaml 的交互逻辑
    /// </summary>


    public partial class index : Page
    {
        ManualResetEvent Thread_blocking = new ManualResetEvent(true);//线程阻塞
        CoverFlow coverFlow = new CoverFlow();
        int Download_Complete = 0;
        public index()
        {
            InitializeComponent();
            Thread thread = new Thread(Home_Recommendation);//加载推荐视频的函数加入一个新的子线程
            thread.Start();//线程开始
            Thread thread2 = new Thread(Homepage_Ad);//加载推荐视频的函数加入一个新的子线程
            thread2.Start();//线程开始
            CoverFlow coverFlow = new CoverFlow();
            coverFlow.Margin= new Thickness(32);
            coverFlow.Width = 800;
            coverFlow.Height = 260;
            coverFlow.SetValue(Grid.RowProperty, 0);
            coverFlow.Loop = true;
            Index_Grid.Children.Add(coverFlow);



        }

        //主页广告
        private void Homepage_Ad()
        {
            
            var client = new RestClient("https://app.bilibili.com/x/v2/feed/index?idx=0&flush=0&column=4&device=pad&pull=true&build=5520400&appkey=4ebafd7c4951b366&mobi_app=iphone&platform=ios&ts=1596754509&access_key=71626a38ce02b8a18b9a8b0dd8add481&sign=5d3bbfa008082c409bf56603278ea7a8");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
            JToken items = recommend["data"]["items"][0]["banner_item"];
            for (int i = 0; i <= items.Count() - 1; i++)
            {

                if (File.Exists(@"Data\Cache\Img\Cover\" + items[i]["id"].ToString() + ".jpg"))
                {
                    coverFlow.Add(new Uri(@"Data\Cache\Img\Cover\" + items[i]["id"].ToString() + ".jpg", UriKind.Relative));
                    Download_Complete++;
                }
                else
                {
                    
                    WebClient Client = new WebClient();
                    Uri uri = new Uri(items[i]["image"].ToString());
                    Client.DownloadFileCompleted += Down_Cover_Completed;
                    Client.DownloadFileAsync(uri, @"Data\Cache\Img\Cover\"+ items[i]["id"].ToString() + ".jpg", @"Data\Cache\Img\Cover\" + items[i]["id"].ToString() + ".jpg");

                }
            }
            
            new Thread((obj) =>
            { 
            while(true)
                {
                    if (Download_Complete ==(int)obj)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                        {
                            coverFlow.JumpTo(1);
                            Index_Grid.Children.Add(coverFlow);
                        });
                        break;
                    }
                }        
            }).Start(items.Count());
            client = null;
            request = null;
            response = null;
            recommend = null;
        }

        //广告封面下载完成
        private void Down_Cover_Completed(object sender, AsyncCompletedEventArgs e)
        {
           
            if (e.UserState != null)
            {
                coverFlow.Add(new Uri(e.UserState.ToString(), UriKind.Relative));
                Download_Complete++;
            }
        }

        //主页推荐
        private void Home_Recommendation()
        {
            while (true)
            {
                Thread_blocking.WaitOne();
                var client = new RestClient("https://app.bilibili.com/x/v2/feed/index?device=pad&mobi_app=iphone");
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                JObject recommend = (JObject)JsonConvert.DeserializeObject(response.Content);
                JToken items = recommend["data"]["items"];
                for (int i = 1; i < items.Count() - 1; i++)
                {
                    new Thread((obj) =>
                    {
                        List<double_row_video> double_row_video = new List<double_row_video>
                        {
                            new double_row_video(
                      items[obj]["avatar"]["cover"].ToString()+"@22w_22h_1c_95q",//up头像
                      items[obj]["desc"].ToString(),//up名字
                      items[obj]["cover"].ToString()+"@320w_200h_1c_95q",//封面
                      items[obj]["cover_left_text_1"].ToString(),//时长
                      items[obj]["title"].ToString().Length>17?items[obj]["title"].ToString().Substring(0,17)+"...":items[obj]["title"].ToString(),//标题
                      items[obj]["args"]["rname"].ToString(),//分区
                      items[obj]["cover_left_text_2"].ToString(),//播放量
                      items[obj]["cover_left_text_3"].ToString()//弹幕数
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
            if (index_scrollViewer.ScrollableHeight == index_scrollViewer.ContentVerticalOffset && content_box.Items.Count >= 18)
                Thread_blocking.Set();


        }

        private void content_box_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            this.content_box.RaiseEvent(eventArg);
        }



    }
}
