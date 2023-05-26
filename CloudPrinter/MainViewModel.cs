using Knd.Printer.Abstract;
using Knd.Printer.Common.Helper;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows;
using CloudPrinter.View;
using CloudPrinter.View.ErrorViews;
using CloudPrinter.ViewModel;
using CloudPrinter.ViewModel.ErrorViews;
using GalaSoft.MvvmLight.Messaging;
using Knd.Printer.SecondApi;
using Ost.AutoPrinter.Api;

namespace CloudPrinter
{
    [Export]
    public class MainViewModel : ViewModelBase
    {
        private GridLength _leftWidth = new GridLength(0.0, GridUnitType.Star);

        private ObservableCollection<PrinterDevice> _printers;

        private ContentControl _currentControl;

        private ContentControl _leftControl;

        private string _businessMenInfo;

        public WaitCardView WaitCardView;

        public PassWordView PassWordView;

        public LightHelp _lightHelp;

        public string BusinessMenInfo
        {
            get
            {
                return _businessMenInfo;
            }
            set
            {
                _businessMenInfo = value;
                NotifyPropertyChanged("BusinessMenInfo");
            }
        }

        public ContentControl LeftControl
        {
            get
            {
                return _leftControl;
            }
            set
            {
                _leftControl = value;
                NotifyPropertyChanged("LeftControl");
            }
        }

        public GridLength LeftWidth
        {
            get
            {
                return _leftWidth;
            }
            set
            {
                _leftWidth = value;
                NotifyPropertyChanged("LeftWidth");
            }
        }

        public ObservableCollection<PrinterDevice> Printer
        {
            get
            {
                return _printers;
            }
            set
            {
                _printers = value;
                NotifyPropertyChanged("Printer");
            }
        }

        public ContentControl CurrentControl
        {
            get
            {
                return _currentControl;
            }
            set
            {
                _currentControl = value;
                NotifyPropertyChanged("CurrentControl");
            }
        }

        public RelayCommand GotoSettingCommand { get; private set; }

        public override async void InitaDataAsync()
        {
            bool HasException = false;
            CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<InitView>();
            if (Printer == null)
            {
                Printer = new ObservableCollection<PrinterDevice>();
            }
            IEnumerable<IBaseDevice> devices = RunTimeHost.MEFContainer.GetExportedValues<IBaseDevice>();
            foreach (IBaseDevice baseDevice in devices)
            {
                if (baseDevice.GetType().BaseType == typeof(PrinterDevice))
                {
                    Printer.Add((PrinterDevice)baseDevice);
                }
            }
            PassWordView = RunTimeHost.MEFContainer.GetExportedValue<PassWordView>();
            Task task = RunTimeHost.MEFContainer.GetExportedValue<InitViewModel>().Start();
            try
            {
                await task;
            }
            catch (Exception e)
            {
                LogManager.AddLog(e);
                string errorMsg = e.Message + "\r";
                if (e.InnerException != null)
                {
                    errorMsg = errorMsg + e.InnerException.Message + "\r";
                    if (e.InnerException.InnerException != null)
                    {
                        errorMsg = errorMsg + e.InnerException.InnerException.Message + "\r";
                    }
                }
                FatalErrorView fatalErrorView = RunTimeHost.MEFContainer.GetExportedValue<FatalErrorView>();
                RunTimeHost.MEFContainer.GetExportedValue<FatalErrorViewModel>().ErrorMsg = errorMsg;
                CurrentControl = fatalErrorView;
                HasException = true;
            }
            RegistMessage();
            AudioHelper.Speak("欢迎使用自助打印系统");
            WaitCardView = RunTimeHost.MEFContainer.GetExportedValue<WaitCardView>();
            MainWindow window = RunTimeHost.MEFContainer.GetExportedValue<MainWindow>();
            ConfigManager<Setting>.Load();
            BusinessMenInfo = ConfigManager<Setting>.Config.BusinessMenInfo;
            VisualStateManager.GoToElementState(window, "Normal", useTransitions: false);
            if (!HasException)
            {
                CurrentControl = WaitCardView;
            }
            base.InitaDataAsync();
        }

        private void _testPrinterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CurrentControl.GetType() == typeof(WaitCardView))
            {
                string num = "23242343";
                Messenger.Default.Send(num, "CardNum");
            }
        }

        public override void InitCommand()
        {
            GotoSettingCommand = new RelayCommand(delegate
            {
                if (CurrentControl.GetType() != typeof(PrintingView))
                {
                    CurrentControl = PassWordView;
                }
            }, () => CurrentControl == WaitCardView || CurrentControl.GetType() == typeof(FatalErrorView));
            base.InitCommand();
        }

        private void RegistMessage()
        {
            Messenger.Default.Register(this, "PrinterCommunicationError", delegate (string errorMsg)
            {
                RunTimeHost.MainDispatcher.Invoke(delegate
                {
                    FatalErrorView exportedValue4 = RunTimeHost.MEFContainer.GetExportedValue<FatalErrorView>();
                    if (CurrentControl != null && (CurrentControl.GetType() == typeof(WaitCardView) || CurrentControl.GetType() == typeof(PrintedView) || CurrentControl.GetType() == typeof(FatalErrorView)))
                    {
                        RunTimeHost.MEFContainer.GetExportedValue<FatalErrorViewModel>().ErrorMsg = errorMsg;
                        CurrentControl = exportedValue4;
                    }
                });
            });
            Messenger.Default.Register(this, "FujiPrinterCommunicationError", delegate (string errorMsg)
            {
                RunTimeHost.MainDispatcher.Invoke(delegate
                {
                    if (CurrentControl != null && CurrentControl.GetType() == typeof(WaitCardView))
                    {
                        ErrorView exportedValue3 = RunTimeHost.MEFContainer.GetExportedValue<ErrorView>();
                        RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().ErrorMsg = errorMsg;
                        RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().InitaDataAsync();
                        CurrentControl = exportedValue3;
                    }
                });
            });
            Messenger.Default.Register(this, "LightControl", delegate (string msg)
            {
                RunTimeHost.MainDispatcher.Invoke(delegate
                {
                    ConfigManager<Setting>.Load();
                    if (_lightHelp != null && ConfigManager<Setting>.Config.EnableLight)
                    {
                        if (msg == "Printing")
                        {
                            _lightHelp.PrintingLight();
                        }
                        else if (msg == "PrintingFinish")
                        {
                            _lightHelp.PrintingFinishLight();
                        }
                        else if (msg == "WaitingPrint")
                        {
                            _lightHelp.WaitingPrintLight();
                        }
                    }
                });
            });
            Messenger.Default.Register<object>(this, "WaitCardView", delegate
            {
                if (CurrentControl.GetType() == typeof(FatalErrorView))
                {
                    WaitCardView = RunTimeHost.MEFContainer.GetExportedValue<WaitCardView>();
                    CurrentControl = WaitCardView;
                }
            });
            Messenger.Default.Register<object>(this, "ReconnectionWaitCardView", delegate
            {
                FatalErrorViewModel exportedValue2 = RunTimeHost.MEFContainer.GetExportedValue<FatalErrorViewModel>();
                if (exportedValue2.ErrorMsg == "胶片打印软件不在线" && CurrentControl.GetType() == typeof(FatalErrorView))
                {
                    WaitCardView = RunTimeHost.MEFContainer.GetExportedValue<WaitCardView>();
                    CurrentControl = WaitCardView;
                }
            });
            Messenger.Default.Register(this, "SendClientInfoToServer", delegate (string[] o)
            {
                try
                {
                    ConfigManager<Setting>.Load();
                    string clientId = ConfigManager<Setting>.Config.ClientId;
                    if (ConfigManager<Setting>.Config.IsOpenSelfService)
                    {
                        List<PaperInfo> list = new List<PaperInfo>();
                        PrintState printState = PrintState.Wating;
                        string stateMsg = null;
                        if (o != null && o.Length != 0)
                        {
                            string[] array = o[0].Split('-');
                            if (array != null && array.Length != 0)
                            {
                                string[] array2 = array;
                                foreach (string text in array2)
                                {
                                    if (text != null && text != "")
                                    {
                                        string[] array3 = text.Split(',');
                                        PaperInfo item = new PaperInfo(array3[0], array3[1], Convert.ToInt32(array3[2]));
                                        list.Add(item);
                                    }
                                }
                            }
                            switch (o[1])
                            {
                                case "就绪":
                                    printState = PrintState.Wating;
                                    break;
                                case "打印中":
                                    printState = PrintState.Printing;
                                    break;
                                case "错误":
                                    printState = PrintState.Error;
                                    break;
                                case "警告":
                                    printState = PrintState.Error;
                                    break;
                                case "未知":
                                    printState = PrintState.Error;
                                    break;
                            }
                            stateMsg = o[2];
                        }
                        SecondServiceApi exportedValue = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>();
                        exportedValue.SendClientInfoToServer(clientId, list.ToArray(), printState, stateMsg);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddLog("主界面，向服务端发送终端状态出错" + ex);
                }
            });
        }
    }
}
