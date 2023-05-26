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
using CloudPrinter.ViewModel;
using Knd.Printer.CoreLibFrame45;

namespace CloudPrinter.View
{

    [Export]
    /// <summary>
    /// WaitCardView.xaml 的交互逻辑
    /// </summary>
    public partial class WaitCardView : UserControl
    {
        [ImportingConstructor]
        public WaitCardView(WaitCardViewModel viewModel)
        {
            base.DataContext = viewModel;
            InitializeComponent();
            base.Loaded += WaitView_Loaded;
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\winting_page.wmv";
            try
            {
                MediaAnimation.Source = new Uri(path);
            }
            catch (Exception ex)
            {
                LogManager.AddLog("动画加载异常" + ex.Message);
            }
        }

        private void WaitView_Loaded(object sender, RoutedEventArgs e)
        {
            sCode.Focus();
            sCode.Text = "";
        }

        private void keyboard_Click(object sender, RoutedEventArgs e)
        {
            MyMediaElement.Visibility = Visibility.Hidden;
            stackpanel1.Visibility = Visibility.Visible;
        }

        private void guanbi_Click(object sender, RoutedEventArgs e)
        {
            stackpanel1.Visibility = Visibility.Hidden;
            MyMediaElement.Visibility = Visibility.Visible;
        }

        private void btn0_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "0";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "1";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void shanchu_Click(object sender, RoutedEventArgs e)
        {
            if (sCode.Text.Length > 0)
            {
                sCode.Text = sCode.Text.Remove(sCode.Text.Length - 1, 1);
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "2";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "3";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn6_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "6";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "5";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "4";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn7_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "7";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn8_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "8";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private void btn9_Click(object sender, RoutedEventArgs e)
        {
            sCode.Text += "9";
            sCode.SelectionStart = sCode.Text.Length;
        }

        private async void chaxun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WaitCardViewModel viewModel = (WaitCardViewModel)base.DataContext;
                await viewModel.KeyDownAsync(Key.Return);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
