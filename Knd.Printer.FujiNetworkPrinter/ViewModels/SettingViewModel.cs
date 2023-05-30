using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.FujiNetworkPrinter.Models;
using Microsoft.Win32;

namespace Knd.Printer.FujiNetworkPrinter.ViewModels
{
    [Export]
    public class SettingViewModel : ViewModelBase
    {
        

        private FujiNetworkPrinter _fujiSerialPrinter;

       

      

        private int _RefreshTime;

    

        public FujiNetworkPrinter FujiSerialPortPrinterr
        {
            get
            {
                return _fujiSerialPrinter;
            }
            set
            {
                _fujiSerialPrinter = value;
                NotifyPropertyChanged("FujiSerialPortPrinterr");
            }
        }

      

       

  

       

        public int RefreshTime
        {
            get
            {
                return _RefreshTime;
            }
            set
            {
                _RefreshTime = value;
                NotifyPropertyChanged("RefreshTime");
            }
        }


        private int _port;

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                NotifyPropertyChanged("Port");
            }
        }
        private string _ip;

        public string Ip
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isFuji8000;
        public bool IsFuji8000
        {
            get
            {
                return _isFuji8000;
            }
            set
            {
                _isFuji8000 = value;
                NotifyPropertyChanged();
            }
        }
        public RelayCommand SaveCommand { get; private set; }

       
        public override void InitCommand()
        {
           
            SaveCommand = new RelayCommand(async delegate
            {
               
                ConfigManager<FujiNetworkConfig>.Config.IsOpen = FujiSerialPortPrinterr.IsOpenStateView;
              
                ConfigManager<FujiNetworkConfig>.Config.RefreshTime = RefreshTime;
                ConfigManager<FujiNetworkConfig>.Config.IpAddress = Ip;
                ConfigManager<FujiNetworkConfig>.Config.Port = Port;
                ConfigManager<FujiNetworkConfig>.Config.IsFuji8000 = IsFuji8000;
                await ConfigManager<FujiNetworkConfig>.Save();
                ConfigManager<FujiNetworkConfig>.Load();
                StateViewModel stateViewModel = RunTimeHost.MEFContainer.GetExportedValue<StateViewModel>();
                stateViewModel.Inita();
            }, () => base.IsValid);
         
            base.InitCommand();
        }

        public async void Inita()
        {
            await ConfigManager<FujiNetworkConfig>.LoadAsync();
        
            RefreshTime = ConfigManager<FujiNetworkConfig>.Config.RefreshTime;
            Ip = ConfigManager<FujiNetworkConfig>.Config.IpAddress;
            Port = ConfigManager<FujiNetworkConfig>.Config.Port;
            IsFuji8000 = ConfigManager<FujiNetworkConfig>.Config.IsFuji8000;
            base.InitaDataAsync();
        }
    }

}
