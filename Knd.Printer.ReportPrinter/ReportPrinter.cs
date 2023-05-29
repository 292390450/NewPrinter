// Knd.Printer.ReportPrinter.ReportPrinter
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Controls;
using Knd.Printer.Abstract;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;
using Knd.Printer.ReportPrinter.Config;
using Knd.Printer.ReportPrinter.View;
using Knd.Printer.ReportPrinter.ViewModel;
using Newtonsoft.Json;
using Ost.AutoPrinter.Api;

namespace Knd.Printer.ReportPrinter
{


    [Export(typeof(IBaseDevice))]
    public class ReportPrinter : PrinterDevice, INotifyPropertyChanged
    {
        private Assembly _currentAssembly;

        private string basePath;

        private Timer _timer;

        private ReportPrinterState[] states;

        private ReportPrinterState _currentState;

        private PrinterState _state;

        private static readonly PrintDocument pd = new PrintDocument();

        private int _jobCount;

        private string _stateStr;

        public int JobCount
        {
            get
            {
                return _jobCount;
            }
            set
            {
                _jobCount = value;
                NotifyPropertyChanged("JobCount");
            }
        }

        public string StateStr
        {
            get
            {
                return _stateStr;
            }
            set
            {
                _stateStr = value;
                NotifyPropertyChanged("StateStr");
            }
        }

        public ReportPrinterState CurrentState
        {
            get
            {
                return _currentState;
            }
            set
            {
                _currentState = value;
                NotifyPropertyChanged("CurrentState");
            }
        }

        public override PrinterState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                NotifyPropertyChanged("State");
            }
        }

        public override PrinterType Type { get; set; }

        public override string Name { get; set; } = "报告打印机";


        [Import(typeof(SettingView))]
        public override ContentControl SettingView { get; set; }

        [Import(typeof(StateView))]
        public override ContentControl StateView { get; set; }

        public override Action Selected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override void Check()
        {
        }

        public override void Start()
        {
            _timer?.Start();
        }

        public override void Stop()
        {
            _timer.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        public override void ResetFilmCountAsync(string num)
        {
        }

        public override void BeginTaskPrint(IList<PrintTaskInfo> taskInfos)
        {
        }

        public override void EndTaskPrint(IList<PrintTaskInfo> taskInfos, bool isSuccess)
        {
        }

        public override void BeginDocumentPrint(AbstractDocumentInfo documentInfo)
        {
        }

        public override void EndDocumentPrint(AbstractDocumentInfo documentInfo, bool isSuccess)
        {
        }

        public override async void Initial()
        {
            _currentAssembly = GetType().Assembly;
            basePath = _currentAssembly.Location.Replace(_currentAssembly.ManifestModule.Name, "");
            string configPath = basePath + "Config\\" + GetType().Name + ".json";
            ConfigManager<ReportPrinterConfig>.Init(configPath, ConfigType.Json, isLog: true);
            if (!File.Exists(configPath))
            {
                ConfigManager<ReportPrinterConfig>.Config = new ReportPrinterConfig
                {
                    RefreshTime = 6000,
                    Name = "Microsoft XPS Document Writer"
                };
                await ConfigManager<ReportPrinterConfig>.Save();
                ConfigManager<ReportPrinterConfig>.Load();
            }
            else
            {
                ConfigManager<ReportPrinterConfig>.Load();
            }
            if (File.Exists(basePath + "Config\\State.txt"))
            {
                string re = File.ReadAllText(basePath + "Config\\State.txt");
                states = JsonConvert.DeserializeObject<ReportPrinterState[]>(re);
            }
            _timer = new Timer(ConfigManager<ReportPrinterConfig>.Config.RefreshTime);
            _timer.Elapsed += _timer_Elapsed;
            StateViewModel viewmodel = RunTimeHost.MEFContainer.GetExportedValue<StateViewModel>();
            viewmodel.ReportPrinter = this;
            SettingViewModel settingViewModel = RunTimeHost.MEFContainer.GetExportedValue<SettingViewModel>();
            settingViewModel.SettingViewModelInit();
            settingViewModel.ReportPrinter = this;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PrinterMonitorInfo info = new PrinterMonitorInfo();
            GetPrinterStatusWMI(ConfigManager<ReportPrinterConfig>.Config.Name, ref info);
            CurrentState = states.FirstOrDefault((ReportPrinterState x) => x.Code == info.StateCode);
            JobCount = info.JobCount;
            StateStr = CurrentState?.Msg;
            switch (CurrentState?.PState)
            {
                case 1:
                    State = PrinterState.就绪;
                    break;
                case 2:
                    State = PrinterState.打印中;
                    break;
                case 3:
                    State = PrinterState.打印中;
                    break;
                default:
                    State = PrinterState.错误;
                    break;
            }
        }

        private bool GetPrinterStatusWMI(string sPrinterName, ref PrinterMonitorInfo ptInfo)
        {
            try
            {
                if (ptInfo == null)
                {
                    ptInfo = new PrinterMonitorInfo();
                }
                if (string.IsNullOrEmpty(sPrinterName))
                {
                    sPrinterName = pd.PrinterSettings.PrinterName;
                }
                string path = "win32_printer.DeviceId='" + sPrinterName + "'";
                ManagementObject printer = new ManagementObject(path);
                printer.Get();
                uint state = 0u;
                int status = Convert.ToInt32(printer.Properties["PrinterStatus"].Value);
                state = (Convert.ToBoolean(printer.Properties["WorkOffline"].Value) ? uint.MaxValue : (status switch
                {
                    0 => 255u,
                    3 => 65535u,
                    _ => Convert.ToUInt32(printer.Properties["PrinterState"].Value),
                }));
                ptInfo.JobCount = Convert.ToInt32(printer.Properties["JobCountSinceLastReset"].Value);
                ptInfo.StateCode = state;
            }
            catch (Exception ex)
            {
                LogManager.AddLog(ex);
            }
            return true;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
