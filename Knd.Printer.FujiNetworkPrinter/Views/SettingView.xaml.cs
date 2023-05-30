using System.ComponentModel.Composition;
using System.Windows.Controls;
using Knd.Printer.FujiNetworkPrinter.ViewModels;

namespace Knd.Printer.FujiNetworkPrinter.Views
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    [Export]
    public partial class SettingView : UserControl
    {
        [ImportingConstructor]
        public SettingView(SettingViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
        }
    }
}
