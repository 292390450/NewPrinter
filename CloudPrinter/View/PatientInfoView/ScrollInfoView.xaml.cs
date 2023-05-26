using CloudPrinter.ViewModel.PatientInfoView;
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

namespace CloudPrinter.View.PatientInfoView
{

    [Export]
    /// <summary>
    /// ScrollInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollInfoView : UserControl
    {
        private TranslateTransform _listTransform;
        [ImportingConstructor]
        public ScrollInfoView(ScrollInfoViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
            _listTransform = new TranslateTransform();
            InfoList.RenderTransform = _listTransform;
        }

        private void ScrollInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            double Offset = 0.0;
            DispatcherTimer timer1 = new DispatcherTimer(TimeSpan.FromMilliseconds(33.0), DispatcherPriority.Normal, delegate
            {
                double actualHeight = VisiualGrid.ActualHeight;
                double actualHeight2 = InfoList.ActualHeight;
                Border border = (Border)VisualTreeHelper.GetChild(InfoList, 0);
                ScrollViewer scrollViewer = (ScrollViewer)border.Child;
                Offset += 2.0;
                if (Offset > scrollViewer.ScrollableHeight)
                {
                    Offset = 0.0;
                }
                scrollViewer.ScrollToVerticalOffset(Offset);
            }, base.Dispatcher);
        }

    }
}
