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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CloudPrinter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]

    public partial class MainWindow : Window
    {
        [ImportingConstructor]
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
            base.Loaded += MainWindow_Loaded;
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\logo.png";
            try
            {
                ImageSource imageSource = new BitmapImage(new Uri(path));
                TopLeftLogo.Source = imageSource;
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
                    Date.Text = DateTime.Now.Year + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
                    Time.Text = DateTime.Now.ToLongTimeString();
                }
                catch (Exception ex)
                {
                    LogManager.AddLog("显示时间异常" + ex.Message);
                }
            }, base.Dispatcher);
        }
    }
}
