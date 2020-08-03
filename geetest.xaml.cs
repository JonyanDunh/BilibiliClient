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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bilibili_Client
{
    /// <summary>
    /// geetest.xaml 的交互逻辑
    /// </summary>
    /// 

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]//com+可见

    public class ObjectForScriptingHelper
    {
        geetest page;

        public ObjectForScriptingHelper(geetest main)
        {
            page = main;
        }

        //这个方法就是网页上要反问的方法
        public void SendKey(string validateg, string seccode)
        {
            page.verification_keys.validate = validateg;
            page.verification_keys.seccode = seccode;
            page.geetest_SendKey_To_Login_page(page.verification_keys);

        }
    }

    public partial class geetest : Page
    {
        public delegate void Geetest_SendKey_To_Login_page(Bilibili.Verification_Key verification_key);
        public Geetest_SendKey_To_Login_page geetest_SendKey_To_Login_page;

        public Bilibili.Verification_Key verification_keys = new Bilibili.Verification_Key();

        public geetest()
        {
            InitializeComponent();
            SetWebBrowserFeatures();//IE11
            
        }
        public void Geetest_Get_Key_From_Login_Page(Bilibili.Verification_Key verification_key)
        {
            verification_keys = verification_key;
            ObjectForScriptingHelper helper = new ObjectForScriptingHelper(this);
            geetest_web.ObjectForScripting = helper;
            //geetest_web.Navigate(@"file:///D:/Github/geetest-validator/geetest.html?challenge="+ verification_key.challenge);
            geetest_web.Source = new Uri(@"pack://siteoforigin:,,,/Web/geetest-validator/geetest.html?challenge=" + verification_key.challenge);
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
