using CloudPrinter.ViewModel.SettingItems;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;
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

namespace CloudPrinter.View.SettingItems
{
    [Export]
    /// <summary>
    /// PatienInfoSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class PatienInfoSettingView : UserControl
    {

        [ImportingConstructor]
        public PatienInfoSettingView(PatienInfoSettingViewModel viewModel)
        {
            base.DataContext = viewModel;
            InitializeComponent();
            base.Loaded += PatienInfoSettingView_Loaded;
        }
        private void PatienInfoSettingView_Loaded(object sender, RoutedEventArgs e)
        {
            ColorPicker.SelectedColor = ConfigManager<Setting>.Config.InfoFontColor;
        }

    }
}
