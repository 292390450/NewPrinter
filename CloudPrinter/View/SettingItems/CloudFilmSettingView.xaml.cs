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
using CloudPrinter.ViewModel.SettingItems;

namespace CloudPrinter.View.SettingItems
{
    [Export]
    /// <summary>
    /// CloudFilmSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class CloudFilmSettingView : UserControl
    {
        [ImportingConstructor]
        public CloudFilmSettingView(CloudFilmSettingViewModel viewModel)
        {
            InitializeComponent();
            base.DataContext = viewModel;
        }

    }
}
