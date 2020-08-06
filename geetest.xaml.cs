using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bilibili_Client
{
    /// <summary>
    /// geetest.xaml 的交互逻辑
    /// </summary>
    /// 
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]

    public class ObjectForScriptingHelper
    {
        geetest page;
        public ObjectForScriptingHelper(geetest main)
        {
            page = main;
        }
        public void SendKey(string validateg, string seccode)//网页回调
        {
            page.verification_keys.validate = validateg;
            page.verification_keys.seccode = seccode;

            if (page.verification_keys.Sms_type == 0)
            {
                this.page.Hide();
                page.geetest_SendKey_To_Login_page(page.verification_keys);
            }
            else
            {
                this.page.Hide();
                page.geetest_SendSmsKey_To_Login_page(page.verification_keys);
            }
            
        }
    }
    public partial class geetest : Window
    {
        public delegate void Geetest_SendKey_To_Login_page(Bilibili_Login.Verification_Key verification_key);
        public Geetest_SendKey_To_Login_page geetest_SendKey_To_Login_page;
        public Geetest_SendKey_To_Login_page geetest_SendSmsKey_To_Login_page;

        public Bilibili_Login.Verification_Key verification_keys = new Bilibili_Login.Verification_Key();
        public geetest()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;//显示位置屏幕居中
            SetWebBrowserFeatures();//IE11
        }
        public void Geetest_Get_Key_From_Login_Page(Bilibili_Login.Verification_Key verification_key)
        {
            verification_keys = verification_key;
            ObjectForScriptingHelper helper = new ObjectForScriptingHelper(this);
            geetest_web.ObjectForScripting = helper;
            geetest_web.Source = new Uri(@"pack://siteoforigin:,,,/resource/Web/geetest-validator/geetest.html?challenge=" + verification_keys.challenge + "&gt=" + verification_keys.gt);
        }
        static void SetWebBrowserFeatures()
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            UInt32 ieMode = (UInt32)11001;
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";
            Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION",
                appName, ieMode, RegistryValueKind.DWord);
            Registry.SetValue(featureControlRegKey + "FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION",
                appName, 1, RegistryValueKind.DWord);
        }
    }
}
