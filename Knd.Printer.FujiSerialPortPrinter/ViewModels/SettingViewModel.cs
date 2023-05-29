using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.FujiSerialPortPrinter.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knd.Printer.CoreLibFrame45.Command;

namespace Knd.Printer.FujiSerialPortPrinter.ViewModels
{
    [Export]
    public class SettingViewModel : ViewModelBase
    {
        private ObservableCollection<string> _sizeCollection;

        private ObservableCollection<FilmBoxModel> _filmBoxs;

        private FujiSerialPortPrinterr _fujiSerialPrinter;

        private string _ComName;

        private int _ComRate;

        private int _RefreshTime;

        private ObservableCollection<string> _comNames;

        public FujiSerialPortPrinterr FujiSerialPortPrinterr
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

        public ObservableCollection<string> SizeCollection
        {
            get
            {
                return _sizeCollection;
            }
            set
            {
                _sizeCollection = value;
                NotifyPropertyChanged("SizeCollection");
            }
        }

        public ObservableCollection<FilmBoxModel> FilmBoxs
        {
            get
            {
                return _filmBoxs;
            }
            set
            {
                _filmBoxs = value;
                NotifyPropertyChanged("FilmBoxs");
            }
        }

        public string ComName
        {
            get
            {
                return _ComName;
            }
            set
            {
                _ComName = value;
                NotifyPropertyChanged("ComName");
            }
        }

        public int ComRate
        {
            get
            {
                return _ComRate;
            }
            set
            {
                _ComRate = value;
                NotifyPropertyChanged("ComRate");
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

        public RelayCommand AddCommand { get; private set; }

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand<FilmBoxModel> ResetCount { get; private set; }

        public RelayCommand<FilmBoxModel> DelCommand { get; private set; }

        public override void InitCommand()
        {
            ResetCount = new RelayCommand<FilmBoxModel>(async delegate (FilmBoxModel filmBox)
            {
                filmBox.Count = filmBox.MaxCount;
                ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs = FilmBoxs.ToArray();
                await ConfigManager<FujiPrinterConfigModel>.Save();
                ConfigManager<FujiPrinterConfigModel>.Load();
                StateViewModel stateViewModel2 = RunTimeHost.MEFContainer.GetExportedValue<StateViewModel>();
                stateViewModel2.Inita();
            });
            AddCommand = new RelayCommand(delegate
            {
                if (FilmBoxs.Count < 4)
                {
                    FilmBoxs.Add(new FilmBoxModel
                    {
                        Size = "14INX17IN",
                        Count = 100,
                        MaxCount = 100
                    });
                }
            });
            SaveCommand = new RelayCommand(async delegate
            {
                ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs = FilmBoxs.ToArray();
                ConfigManager<FujiPrinterConfigModel>.Config.IsOpen = FujiSerialPortPrinterr.IsOpenStateView;
                ConfigManager<FujiPrinterConfigModel>.Config.ComName = ComName;
                ConfigManager<FujiPrinterConfigModel>.Config.IsOpenSerialPort = FujiSerialPortPrinterr.IsOpenSerialProt;
                ConfigManager<FujiPrinterConfigModel>.Config.RefreshTime = RefreshTime;
                await ConfigManager<FujiPrinterConfigModel>.Save();
                ConfigManager<FujiPrinterConfigModel>.Load();
                StateViewModel stateViewModel = RunTimeHost.MEFContainer.GetExportedValue<StateViewModel>();
                stateViewModel.Inita();
            }, () => base.IsValid);
            DelCommand = new RelayCommand<FilmBoxModel>(async delegate (FilmBoxModel filmBox)
            {
                FilmBoxs.Remove(filmBox);
                ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs = FilmBoxs.ToArray();
                await ConfigManager<FujiPrinterConfigModel>.Save();
                ConfigManager<FujiPrinterConfigModel>.Load();
            });
            base.InitCommand();
        }

        public async void Inita()
        {
            await ConfigManager<FujiPrinterConfigModel>.LoadAsync();
            ComName = ConfigManager<FujiPrinterConfigModel>.Config.ComName;
            RefreshTime = ConfigManager<FujiPrinterConfigModel>.Config.RefreshTime;
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
            if (SizeCollection == null)
            {
                SizeCollection = new ObservableCollection<string>();
                SizeCollection.Add("14INX17IN");
                SizeCollection.Add("10INX14IN");
                SizeCollection.Add("11INX14IN");
                SizeCollection.Add("10INX12IN");
                SizeCollection.Add("8INX10IN");
            }
            if (FilmBoxs == null)
            {
                FilmBoxs = new ObservableCollection<FilmBoxModel>();
            }
            RunTimeHost.MainDispatcher.Invoke(delegate
            {
                FilmBoxs.Clear();
                FilmBoxs.AddRange(ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs);
            });
            base.InitaDataAsync();
        }
    }

}
