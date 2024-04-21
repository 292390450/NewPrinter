using CloudPrinter.Model;
using Knd.Printer.Abstract;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using Knd.Printer.SecondApi;
using Ost.AutoPrinter.Api.Its;
using Ost.AutoPrinter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows;
using CloudPrinter.View;
using CloudPrinter.View.ErrorViews;
using CloudPrinter.View.PatientInfoView;
using Knd.Printer.Common.Helper;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class InitViewModel : ViewModelBase
    {
        private string _initText;

        private double _currentProgress;

        public bool IsServerConnected { get; set; }

        public bool IsSignalrConnected { get; set; }

        public string InitText
        {
            get
            {
                return _initText;
            }
            set
            {
                _initText = value;
                NotifyPropertyChanged("InitText");
            }
        }

        public double CurrentProgress
        {
            get
            {
                return _currentProgress;
            }
            set
            {
                _currentProgress = value;
                NotifyPropertyChanged("CurrentProgress");
            }
        }

        public override void InitaDataAsync()
        {
            base.InitaDataAsync();
        }

        public Task Start()
        {
            IEnumerable<IBaseDevice> devices = RunTimeHost.MEFContainer.GetExportedValues<IBaseDevice>();
            List<PrintTaskInfo> info;
            return Task.Run(async delegate
            {
                string configPath = AppDomain.CurrentDomain.BaseDirectory + "\\setting.json";
                ConfigManager<Setting>.Init(configPath, ConfigType.Json, isLog: true);
                if (!File.Exists(configPath))
                {
                    ConfigManager<Setting>.Config = new Setting
                    {
                        ClientId = "P2E40CA26E67EA7CB",
                        TimeOut = 6000,
                        ServerId = "127.0.0.1",
                        Port = 2500,
                        HubName = "ExchangeHub",
                        SignalrUrl = "http://127.0.0.1:10086/",
                        UploadClientId = "71ef57ff728447e6a9aad7bdc6e147a8",
                        ScreenCount = 1,
                        InfoFontColor = Colors.White,
                        IsDeleteTempFile = true
                    };
                    PrintFilmTimeModel printFilmTimeModel = new PrintFilmTimeModel
                    {
                        PrintFilmCount = 1,
                        PrintFilmSize = "14INX17IN",
                        PrintFilmTime = 30
                    };
                    ConfigManager<Setting>.Config.printFilmTimeModels = new List<PrintFilmTimeModel>();
                   // ConfigManager<Setting>.Config.printFilmTimeModels.Add(printFilmTimeModel);
                    ConfigManager<Setting>.Config.NewPrintFilmTimeModels = new List<NewPrintFilmTimeModel>()
                    {
                        new NewPrintFilmTimeModel(){Index = 1,PrintModels = new ObservableCollection<PrintFilmTimeModel>()
                        {
                            printFilmTimeModel
                        }}
                    };
                    await ConfigManager<Setting>.Save();
                }
                else
                {
                    ConfigManager<Setting>.Load();
                }
                if (ConfigManager<Setting>.Config.IsOpenColud)
                {
                    ISignalrServer signalrServer = RunTimeHost.MEFContainer.GetExportedValue<ISignalrServer>();
                    await signalrServer.Init(ConfigManager<Setting>.Config);
                    signalrServer.DisconnectionAction = delegate
                    {
                        IsSignalrConnected = false;
                        RunTimeHost.MainDispatcher.Invoke(delegate
                        {
                            RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<DisConnectionView>();
                        });
                    };
                    signalrServer.Connected = delegate
                    {
                        IsSignalrConnected = true;
                        RunTimeHost.MainDispatcher.Invoke(delegate
                        {
                            RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<WaitCardView>();
                        });
                    };
                    signalrServer.OnReciveMsg = delegate (MqMessageModel model)
                    {
                        RunTimeHost.MainDispatcher.Invoke(delegate
                        {
                            if (RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl == RunTimeHost.MEFContainer.GetExportedValue<OnlyCloudPrintView>())
                            {
                                RunTimeHost.MEFContainer.GetExportedValue<OnlyCloudPrintViewModel>().SetState(model);
                            }
                        });
                    };
                }
                if (ConfigManager<Setting>.Config.IsOpenSelfService)
                {
                    SecondServiceApi secondApi = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>();
                    secondApi.InitRemoteSerivce(ConfigManager<Setting>.Config.ServerId, ConfigManager<Setting>.Config.Port, ConfigManager<Setting>.Config.ClientId, "1254", 1, ConfigManager<Setting>.Config.TimeOut);
                    secondApi.ConnectionChanged = delegate (bool b)
                    {
                        if (!b)
                        {
                            RunTimeHost.MainDispatcher.Invoke(delegate
                            {
                                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<DisConnectionView>();
                            });
                        }
                        else if (!IsServerConnected)
                        {
                            RunTimeHost.MainDispatcher.Invoke(delegate
                            {
                                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<WaitCardView>();
                            });
                        }
                        IsServerConnected = b;
                    };
                    secondApi.OnRecieveMsg = delegate (PrintDocArgs args)
                    {
                        try
                        {
                            ReceiveMessage receiveMessage = new ReceiveMessage(args.TaskInfo, args.PrintAccount);
                            if (receiveMessage.Category == ReceiveMessage.MessageCategory.PrintDoc)
                            {
                                info = new List<PrintTaskInfo> { (PrintTaskInfo)receiveMessage.HandlerParam };
                                RunTimeHost.MainDispatcher.Invoke(delegate
                                {
                                    if (RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl == RunTimeHost.MEFContainer.GetExportedValue<WaitCardView>())
                                    {
                                        RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<PrintingView>();
                                        RunTimeHost.MEFContainer.GetExportedValue<PrintingViewModel>().InitSetTasks(info, PrintType.ReportsAndFilms, new InputPara
                                        {
                                            EqpCode = "SERVER",
                                            ext = "",
                                            Num = info.FirstOrDefault().AccNo
                                        });
                                    }
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            LogManager.AddLog("接收到主动打印事件, 在执行中发生异常");
                            LogManager.AddLog(e);
                        }
                    };
                }
                if (ConfigManager<Setting>.Config.EnableLight && ConfigManager<Setting>.Config.LightCom != null && ConfigManager<Setting>.Config.LightRate != 0)
                {
                    RunTimeHost.MainDispatcher.Invoke(delegate
                    {
                        RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>()._lightHelp = new LightHelp(ConfigManager<Setting>.Config.LightCom, ConfigManager<Setting>.Config.LightRate);
                        RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>()._lightHelp.CheckLight();
                    });
                }
                if (devices.Count() > 0)
                {
                    int indexStep = 100 / (devices.Count() * 3);
                    CurrentProgress = 0.0;
                    foreach (IBaseDevice printerDevice in devices)
                    {
                        InitText = "初始化" + printerDevice.Name;
                        printerDevice.Initial();
                        CurrentProgress += (double)indexStep;
                        Thread.Sleep(200);
                        InitText = "检查" + printerDevice.Name;
                        printerDevice.Check();
                        CurrentProgress += (double)indexStep;
                        Thread.Sleep(200);
                        InitText = "启动" + printerDevice.Name;
                        printerDevice.Start();
                        CurrentProgress += (double)indexStep;
                        Thread.Sleep(200);
                    }
                    CurrentProgress = 100.0;
                    if (ConfigManager<Setting>.Config.ScreenCount < 2)
                    {
                        RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().LeftWidth = new GridLength(1.0, GridUnitType.Star);
                        RunTimeHost.MainDispatcher.Invoke(delegate
                        {
                            RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().LeftControl = RunTimeHost.MEFContainer.GetExportedValue<ScrollInfoView>();
                        });
                    }
                    else
                    {
                        LogManager.AddLog("系统屏幕个数" + Screen.AllScreens.Length);
                        if (Screen.AllScreens.Length > 1)
                        {
                            RunTimeHost.MainDispatcher.Invoke(delegate
                            {
                                TopWindowView exportedValue = RunTimeHost.MEFContainer.GetExportedValue<TopWindowView>();
                                if (Screen.AllScreens.Length > 1)
                                {
                                    Rectangle bounds = Screen.AllScreens[0].Bounds;
                                    LogManager.AddLog($"屏幕1: 上{bounds.Top},左{bounds.Left},宽{bounds.Width},高{bounds.Height}");
                                    Rectangle bounds2 = Screen.AllScreens[1].Bounds;
                                    LogManager.AddLog($"屏幕2: 上{bounds2.Top},左{bounds2.Left},宽{bounds2.Width},高{bounds2.Height}");
                                    switch (ConfigManager<Setting>.Config.ExtenScreenIndex)
                                    {
                                        case 1:
                                            exportedValue.Top = bounds.Top;
                                            exportedValue.Left = bounds.Left;
                                            exportedValue.Width = bounds.Width;
                                            exportedValue.Height = bounds.Height;
                                            break;
                                        case 2:
                                            exportedValue.Top = bounds2.Top;
                                            exportedValue.Left = bounds2.Left;
                                            exportedValue.Width = bounds2.Width;
                                            exportedValue.Height = bounds2.Height;
                                            break;
                                    }
                                }
                                exportedValue.Show();
                            });
                        }
                    }
                }
            });
        }
    }

}
