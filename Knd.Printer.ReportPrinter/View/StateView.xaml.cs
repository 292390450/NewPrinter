﻿using System;
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
using Knd.Printer.ReportPrinter.ViewModel;

namespace Knd.Printer.ReportPrinter.View
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
