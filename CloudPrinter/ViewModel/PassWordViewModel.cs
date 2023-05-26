using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudPrinter.View;
using Knd.Printer.Core;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class PassWordViewModel : ViewModelBase
    {
        private string _passWd;

        public string PassWd
        {
            get
            {
                return _passWd;
            }
            set
            {
                _passWd = value;
                NotifyPropertyChanged("PassWd");
            }
        }

        public RelayCommand<string> ClickCommand { get; private set; }

        public RelayCommand ReturnCommand { get; private set; }

        public override void InitCommand()
        {
            ClickCommand = new RelayCommand<string>(delegate (string str)
            {
                try
                {
                    if (PassWd == "0000")
                    {
                        MainViewModel exportedValue2 = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>();
                        exportedValue2.CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<SettingView>();
                        PassWd = string.Empty;
                        SettingViewModel settingViewModel = (SettingViewModel)exportedValue2.CurrentControl.DataContext;
                        settingViewModel.CanVisibility = "Visible";
                    }
                    else if (PassWd == "1234")
                    {
                        MainViewModel exportedValue3 = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>();
                        exportedValue3.CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<SettingView>();
                        PassWd = string.Empty;
                        SettingViewModel settingViewModel2 = (SettingViewModel)exportedValue3.CurrentControl.DataContext;
                        settingViewModel2.CanVisibility = "Hidden";
                    }
                    else if (string.IsNullOrEmpty(PassWd) || PassWd.Length < 4)
                    {
                        PassWd += str;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddLog("切换设置界面出错:" + ex.Message);
                }
            });
            base.InitCommand();
            ReturnCommand = new RelayCommand(delegate
            {
                MainViewModel exportedValue = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>();
                exportedValue.CurrentControl = exportedValue?.WaitCardView;
                PassWd = string.Empty;
            });
        }
    }
}
