using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace Bilibili_Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow3
    { 

    }

    public partial class MainWindow : Window
    {
        index index_page = new index();
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;//显示位置屏幕居中
            mainwindow.Width = (SystemParameters.PrimaryScreenWidth) * 0.83;
            mainwindow.Height = (SystemParameters.PrimaryScreenHeight) * 0.83;
            left_grid.Width = mainwindow.Width * 0.145;
            right_grid.Width = mainwindow.Width * 0.234;
            
            middle_frame.Navigate(index_page);

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)//移动窗口
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }



        private void index_button(object sender, RoutedEventArgs e)
        {

            if(index_page.index_scrollViewer.ScrollableHeight== index_page.index_scrollViewer.ContentVerticalOffset)
            MessageBox.Show("到底了");



        }


    }
}
