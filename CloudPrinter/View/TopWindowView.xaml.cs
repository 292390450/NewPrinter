using CloudPrinter.ViewModel;
using Knd.Printer.CoreLibFrame45;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CloudPrinter.View
{

    [Export]
    /// <summary>
    /// TopWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class TopWindowView : Window
    {
        [ImportingConstructor]
        public TopWindowView(TopWindowViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
            base.Loaded += MainWindow_Loaded;
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\logo.png";
            try
            {
                ImageSource imageSource = new BitmapImage(new Uri(path));
                ScrollInfoLogo.Source = imageSource;
            }
            catch (Exception ex)
            {
                LogManager.AddLog("logo加载异常" + ex.Message);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0), DispatcherPriority.Normal, delegate
            {
                try
                {
                    TxtTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + TransfWeek(DateTime.Now.DayOfWeek.ToString());
                }
                catch (Exception)
                {
                }
            }, base.Dispatcher);
        }

        private string TransfWeek(string s)
        {
            string temp = string.Empty;
            switch (s.ToLower())
            {
                case "monday":
                    temp = "星期一";
                    break;
                case "tuesday":
                    temp = "星期二";
                    break;
                case "wednesday":
                    temp = "星期三";
                    break;
                case "thursday":
                    temp = "星期四";
                    break;
                case "friday":
                    temp = "星期五";
                    break;
                case "saturday":
                    temp = "星期六";
                    break;
                case "sunday":
                    temp = "星期日";
                    break;
            }
            return temp;
        }
    }
}
