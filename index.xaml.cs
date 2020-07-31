using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bilibili_Client
{
    /// <summary>
    /// index.xaml 的交互逻辑
    /// </summary>
    public partial class index : Page
    {
        public index()
        {
            InitializeComponent();
            List<double_row_video> double_row_videos = new List<double_row_video>();
            double_row_videos.Add(new double_row_video("http://i1.hdslb.com/bfs/face/3dc6737d4ab6e8d0390409af88e9d24c3056a53b.jpg", 
                "红豆稀饭中",
                "算起来做了有一个多月了，从一百多份演讲视频中截取单词最后拼成这首歌，视频也是反反复复修改了半个多月，这是我目前做过难度最大的视频了。喜欢的话三连拜托啦！",
                "https://i2.hdslb.com/bfs/archive/2b71355c3a3166abf2714dd1e982fedb82ed254c.jpg", 
                "“美利坚，你们的皇帝回来了！”",
                "BV1ri4y1u7JR",
                "270.2万",
                "34.8万",
                "22.6万", 
                "14.3万", 
                "2.0万",
                "1.1万",
                "5911"
                ));
            double_row_videos.Add(new double_row_video("http://i1.hdslb.com/bfs/face/3dc6737d4ab6e8d0390409af88e9d24c3056a53b.jpg",
    "红豆稀饭中",
    "算起来做了有一个多月了，从一百多份演讲视频中截取单词最后拼成这首歌，视频也是反反复复修改了半个多月，这是我目前做过难度最大的视频了。喜欢的话三连拜托啦！",
    "https://i0.hdslb.com/bfs/archive/98a45a0b2b8cf2923d451b5a48d7d1114a55d5c5.jpg",
    "“美利坚，你们的皇帝回来了！”",
    "BV1ri4y1u7JR",
    "270.2万",
    "34.8万",
    "22.6万",
    "14.3万",
    "2.0万",
    "1.1万",
    "5911"
    ));
            double_row_videos.Add(new double_row_video("http://i1.hdslb.com/bfs/face/3dc6737d4ab6e8d0390409af88e9d24c3056a53b.jpg",
    "红豆稀饭中",
    "算起来做了有一个多月了，从一百多份演讲视频中截取单词最后拼成这首歌，视频也是反反复复修改了半个多月，这是我目前做过难度最大的视频了。喜欢的话三连拜托啦！",
    "https://i0.hdslb.com/bfs/archive/98a45a0b2b8cf2923d451b5a48d7d1114a55d5c5.jpg",
    "“美利坚，你们的皇帝回来了！”",
    "BV1ri4y1u7JR",
    "270.2万",
    "34.8万",
    "22.6万",
    "14.3万",
    "2.0万",
    "1.1万",
    "5911"
    ));
            double_row_videos.Add(new double_row_video("http://i1.hdslb.com/bfs/face/3dc6737d4ab6e8d0390409af88e9d24c3056a53b.jpg",
    "红豆稀饭中",
    "算起来做了有一个多月了，从一百多份演讲视频中截取单词最后拼成这首歌，视频也是反反复复修改了半个多月，这是我目前做过难度最大的视频了。喜欢的话三连拜托啦！",
    "https://i0.hdslb.com/bfs/archive/98a45a0b2b8cf2923d451b5a48d7d1114a55d5c5.jpg",
    "“美利坚，你们的皇帝回来了！”",
    "BV1ri4y1u7JR",
    "270.2万",
    "34.8万",
    "22.6万",
    "14.3万",
    "2.0万",
    "1.1万",
    "5911"
    ));
            double_row_videos.Add(new double_row_video("http://i1.hdslb.com/bfs/face/3dc6737d4ab6e8d0390409af88e9d24c3056a53b.jpg",
    "红豆稀饭中",
    "算起来做了有一个多月了，从一百多份演讲视频中截取单词最后拼成这首歌，视频也是反反复复修改了半个多月，这是我目前做过难度最大的视频了。喜欢的话三连拜托啦！",
    "https://i0.hdslb.com/bfs/archive/98a45a0b2b8cf2923d451b5a48d7d1114a55d5c5.jpg",
    "“美利坚，你们的皇帝回来了！”",
    "BV1ri4y1u7JR",
    "270.2万",
    "34.8万",
    "22.6万",
    "14.3万",
    "2.0万",
    "1.1万",
    "5911"
    ));

            /*double_row_videos.Add(new double_row_video("First"));
            double_row_videos.Add(new double_row_video("First"));
            double_row_videos.Add(new double_row_video("First"));*/
            content_box.ItemsSource = double_row_videos;
        }
        public class double_row_video
        {
            public string head_img_url { get; private set; }
            public string video_up { get; private set; }
            public string video_introduction { get; private set; }
            public string video_cover { get; private set; }
            public string video_title { get; private set; }
            public string video_bvid { get; private set; }
            public string video_play_volume { get; private set; }
            public string video_likes{ get; private set; }
            public string video_coins { get; private set; }
            public string video_favorites { get; private set; }
            public string video_shares { get; private set; }
            public string video_play_barrages { get; private set; }
            public string video_play_comments { get; private set; }

            public double_row_video(string up_head, string up, string introduction, string cover, string title, string bvid, string play_volume, string likes, string coins, string favorites, string shares, string barrages, string comments)
            {
                head_img_url = up_head;
                video_up = up;
                video_introduction = introduction;
                video_cover = cover;
                video_title = title;
                video_bvid = bvid;
                video_play_volume = play_volume;
                video_likes = likes;
                video_coins = coins;
                video_favorites = favorites;
                video_shares = shares;
                video_play_barrages = barrages;
                video_play_comments = comments;
            }
        }
    }
}
