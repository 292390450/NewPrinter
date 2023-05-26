// CloudPrinter.ViewModel.SettingItems.SystemSettingViewModel

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;
using Microsoft.Win32;

namespace CloudPrinter.ViewModel.SettingItems;

[Export]
public class SystemSettingViewModel : ViewModelBase
{
    private Setting _setting;

    private ObservableCollection<string> _comNames;

    private ObservableCollection<int> _lightRateCollection;

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

    public List<PrintOder> PrintOderEnum { get; set; }= new List<PrintOder>(Enum.GetValues(typeof(PrintOder)).Cast<PrintOder>());
    public ObservableCollection<string> ComNames
    {
        get
        {
            return _comNames;
        }
        set
        {
            _comNames = value;
            NotifyPropertyChanged("ComNames");
        }
    }

    public ObservableCollection<int> LightRateCollection
    {
        get
        {
            return _lightRateCollection;
        }
        set
        {
            _lightRateCollection = value;
            NotifyPropertyChanged("LightRateCollection");
        }
    }

    public RelayCommand SaveCommand { get; private set; }

    public override async void InitaDataAsync()
    {
        await ConfigManager<Setting>.LoadAsync();
        Setting = ConfigManager<Setting>.Config;
        try
        {
            if (ComNames == null)
            {
                ComNames = new ObservableCollection<string>();
                List<string> _namesdata = new List<string>();
                RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
                if (keyCom != null)
                {
                    string[] sSubKeys = keyCom.GetValueNames();
                    string[] array = sSubKeys;
                    foreach (string sName in array)
                    {
                        string sValue = (string)keyCom.GetValue(sName);
                        _namesdata.Add(sValue);
                        LogManager.AddLog("加载本机串口：" + sValue);
                    }
                }
                RunTimeHost.MainDispatcher.Invoke(delegate
                {
                    ComNames.AddRange(_namesdata);
                });
            }
            LogManager.AddLog("加载本机串口成功");
        }
        catch (Exception ex)
        {
            Exception e = ex;
            LogManager.AddLog("加载本机串口失败：" + e.Message);
            throw e;
        }
        if (LightRateCollection == null)
        {
            LightRateCollection = new ObservableCollection<int>();
            LightRateCollection.Add(9600);
            LightRateCollection.Add(14400);
            LightRateCollection.Add(19200);
            LightRateCollection.Add(57600);
            LightRateCollection.Add(115200);
            LightRateCollection.Add(128000);
        }
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
        base.InitCommand();
    }

    public override void RestartApp()
    {
        MessageBoxResult re = System.Windows.MessageBox.Show("保存系统设置软件将会进行重启. 请确认是否保存修改?", "确认保存?", MessageBoxButton.YesNo);
        if (re == MessageBoxResult.Yes)
        {
            System.Windows.Application.Current.Shutdown();
            Thread.Sleep(5000);
            System.Windows.Forms.Application.Restart();
        }
    }
}