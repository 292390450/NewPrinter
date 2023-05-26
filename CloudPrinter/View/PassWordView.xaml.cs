using CloudPrinter.ViewModel;
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

namespace CloudPrinter.View
{
    /// <summary>
    /// PassWordView.xaml 的交互逻辑
    /// </summary>
    [Export]
    public partial class PassWordView : UserControl
    {
        [ImportingConstructor]
        public PassWordView(PassWordViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (passwordbox1.Password.Length > 0)
            {
                passwordbox1.Password = passwordbox1.Password.Remove(passwordbox1.Password.Length - 1, 1);
            }
        }

    }
}
