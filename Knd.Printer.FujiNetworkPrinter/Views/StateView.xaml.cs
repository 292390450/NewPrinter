using System.ComponentModel.Composition;
using System.Windows.Controls;
using Knd.Printer.FujiNetworkPrinter.ViewModels;

namespace Knd.Printer.FujiNetworkPrinter.Views
{
    /// <summary>
    /// StateView.xaml 的交互逻辑
    /// </summary>
    [Export]
    public partial class StateView : UserControl
    {
        [ImportingConstructor]
        public StateView(StateViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
        }
    }
}
