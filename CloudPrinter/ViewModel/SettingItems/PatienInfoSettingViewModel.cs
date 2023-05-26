// CloudPrinter.ViewModel.SettingItems.PatienInfoSettingViewModel

using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;
using Telerik.Windows.Controls;

namespace CloudPrinter.ViewModel.SettingItems;

[Export]
public class PatienInfoSettingViewModel : Knd.Printer.CoreLibFrame45.ViewModelBase
{
    private Setting _setting;

    public Setting Setting
    {
        get
        {
            return _setting;
        }
        set
        {
            _setting = value;
            NotifyPropertyChanged("Setting");
        }
    }

    public RelayCommand SaveCommand { get; private set; }

    public RelayCommand<RadColorPicker> ColorChangeCommand { get; set; }

    public override async void InitaDataAsync()
    {
        await ConfigManager<Setting>.LoadAsync();
        Setting = ConfigManager<Setting>.Config;
        base.InitaDataAsync();
    }

    public override void InitCommand()
    {
        SaveCommand = new RelayCommand(async delegate
        {
            base.IsBusy = true;
            ConfigManager<Setting>.Config = Setting;
            await ConfigManager<Setting>.Save();
            base.IsBusy = false;
            RestartApp();
        }, () => base.IsValid);
        ColorChangeCommand = new RelayCommand<RadColorPicker>(delegate (RadColorPicker picker)
        {
            Setting.InfoFontColor = picker.SelectedColor;
        });
        base.InitCommand();
    }

    public override void RestartApp()
    {
        MessageBoxResult re = System.Windows.MessageBox.Show("保存系统设置软件将会进行重启. 请确认是否保存修改?", "确认保存?", MessageBoxButton.YesNo);
        if (re == MessageBoxResult.Yes)
        {
            Thread.Sleep(1000);
            System.Windows.Application.Current.Shutdown();
            Thread.Sleep(2000);
            System.Windows.Forms.Application.Restart();
        }
    }
}