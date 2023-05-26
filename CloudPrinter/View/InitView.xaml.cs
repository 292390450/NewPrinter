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
using CloudPrinter.ViewModel;

namespace CloudPrinter.View
{
    /// <summary>
    /// InitView.xaml 的交互逻辑
    /// </summary>
    [Export]
    public partial class InitView : UserControl
    {
        [ImportingConstructor]
        public InitView(InitViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\logo.png";
            try
            {
                ImageSource imageSource = new BitmapImage(new Uri(path));
                InitLogo.Source = imageSource;
            }
            catch (Exception ex)
            {
                LogManager.AddLog("logo加载异常" + ex.Message);
            }
        }
    }
}
