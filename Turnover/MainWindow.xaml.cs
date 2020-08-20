using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using mshtml;

namespace Turnover
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            SuppressScriptErrors(web, true);
            web.Navigate($"http://stockapp.finance.qq.com/mstats/?_={DateTime.Now.Ticks}");

            web.Navigated += (o, ex) => {
                Task.Run(() => {
                    Thread.Sleep(4000);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        compute();
                    }));
                });
            };

            timer.Interval = new TimeSpan(0, 0, 6);
            timer.Tick += (o, ex) => {
                SuppressScriptErrors(web, true);
                web.Navigate($"http://stockapp.finance.qq.com/mstats/?_={DateTime.Now.Ticks}");
            };
            timer.Start();
        }

        /// <summary>
        /// 在加载页面之前调用此方法设置hide为true就能抑制错误的弹出了。
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="hide"></param>
        static void SuppressScriptErrors(WebBrowser webBrowser, bool hide)
        {
            webBrowser.Navigating += (s, e) =>
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fiComWebBrowser == null)
                    return;

                object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
                if (objComWebBrowser == null)
                    return;

                objComWebBrowser.GetType().InvokeMember("Silent", System.Reflection.BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            };
        }
        private void compute()
        {
            var doc = web.Document as mshtml.HTMLDocument;
            if (doc == null)
            {
                return;
            }

            IHTMLElement szStr = doc.getElementById("mktinfo-sh000001-cje");
            IHTMLElement scStr = doc.getElementById("mktinfo-sz399001-cje");
            var _sz = szStr.innerHTML.Replace("成交额:", string.Empty).Replace("亿元", string.Empty);
            var _sc = scStr.innerHTML.Replace("成交额:", string.Empty).Replace("亿元", string.Empty);
            sz.Content = Convert.ToInt32(_sz);
            sc.Content = Convert.ToInt32(_sc);
            ze.Content = (Convert.ToInt32(_sz) + Convert.ToInt32(_sc));
        }
    }
}
