using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using Knd.Printer.SecondApi;
using Ost.AutoPrinter.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CloudPrinter.View;
using CloudPrinter.View.ErrorViews;
using CloudPrinter.ViewModel.ErrorViews;
using GalaSoft.MvvmLight.Messaging;
using Knd.Printer.Abstract;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class WaitCardViewModel : ViewModelBase
    {
        private string _num;

        public string Num
        {
            get
            {
                return _num;
            }
            set
            {
                _num = value;
                NotifyPropertyChanged("Num");
            }
        }

        public RelayCommand<KeyEventArgs> KeyDownCommand { get; private set; }

        public WaitCardViewModel()
        {
            RegistMessage();
        }

        public override void InitCommand()
        {
            KeyDownCommand = new RelayCommand<KeyEventArgs>(async delegate (KeyEventArgs args)
            {
                if (!base.IsBusy && args.Key == Key.Return && !string.IsNullOrEmpty(Num))
                {
                    LogManager.AddLog("接受到输入：" + Num);
                    if (Num.StartsWith("CZ") && Num.Length == 6)
                    {
                        IEnumerable<IBaseDevice> devices = RunTimeHost.MEFContainer.GetExportedValues<IBaseDevice>();
                        foreach (IBaseDevice printerDevice in devices)
                        {
                            printerDevice.ResetFilmCountAsync(Num);
                        }
                        Num = "";
                    }
                    else
                    {
                        Num = ClearErrorCode(Num);
                        base.IsBusy = true;
                        try
                        {
                            if (!ConfigManager<Setting>.Config.IsOpenSelfService)
                            {
                                throw new Exception("终端配置未启用自助服务");
                            }
                            IList<PrintTaskInfo> res = await LoadPatientInfo(Num, "01", null);
                            if (ConfigManager<Setting>.Config.IsOpenColud && RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().is_open_pay == 1)
                            {
                                LogManager.AddLog("跳转到云影像确认界面");
                                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<ConfirmView>();
                                RunTimeHost.MEFContainer.GetExportedValue<ConfirmViewModel>().Init(res, new InputPara
                                {
                                    EqpCode = "01",
                                    ext = "",
                                    Num = Num
                                });
                            }
                            else
                            {
                                LogManager.AddLog("从服务器获取打印任务成功，跳转打印界面");
                                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<PrintingView>();
                                RunTimeHost.MEFContainer.GetExportedValue<PrintingViewModel>().InitSetTasks(res, PrintType.ReportsAndFilms, new InputPara
                                {
                                    EqpCode = "01",
                                    ext = "",
                                    Num = Num
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Exception e = ex;
                            LogManager.AddLog(e.Message);
                            ErrorView errorView = RunTimeHost.MEFContainer.GetExportedValue<ErrorView>();
                            RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().ErrorMsg = e.Message;
                            RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().InitaDataAsync();
                            RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = errorView;
                        }
                        base.IsBusy = false;
                    }
                }
            });
            base.InitCommand();
        }

        private Task<IList<PrintTaskInfo>> LoadPatientInfo(string num, string eqp, string msg)
        {
            return RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().GetPrintTaskInfoAsync(num, eqp, msg);
        }

        private void RegistMessage()
        {
            Messenger.Default.Register(this, "CardNum", delegate (string CardNum)
            {
                RunTimeHost.MainDispatcher.Invoke((Func<Task>)async delegate
                {
                    if (RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl.GetType() == typeof(WaitCardView))
                    {
                        LogManager.AddLog("接受到刷卡器检查号，准备打印");
                        Num = CardNum;
                        await KeyDownAsync(Key.Return);
                    }
                });
            });
        }

        public async Task KeyDownAsync(Key key)
        {
            if (base.IsBusy || key != Key.Return || string.IsNullOrEmpty(Num))
            {
                return;
            }
            LogManager.AddLog("接受到输入：" + Num);
            Num = ClearErrorCode(Num);
            base.IsBusy = true;
            try
            {
                IList<PrintTaskInfo> res = await LoadPatientInfo(Num, "01", null);
                if (ConfigManager<Setting>.Config.IsOpenColud && RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().is_open_pay == 1)
                {
                    LogManager.AddLog("跳转到云影像确认界面");
                    RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<ConfirmView>();
                    RunTimeHost.MEFContainer.GetExportedValue<ConfirmViewModel>().Init(res, new InputPara
                    {
                        EqpCode = "01",
                        ext = "",
                        Num = Num
                    });
                }
                else
                {
                    LogManager.AddLog("从服务器获取打印任务成功，跳转打印界面");
                    RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<PrintingView>();
                    RunTimeHost.MEFContainer.GetExportedValue<PrintingViewModel>().InitSetTasks(res, PrintType.ReportsAndFilms, new InputPara
                    {
                        EqpCode = "01",
                        ext = "",
                        Num = Num
                    });
                }
            }
            catch (Exception ex)
            {
                Exception e = ex;
                LogManager.AddLog(e);
                ErrorView errorView = RunTimeHost.MEFContainer.GetExportedValue<ErrorView>();
                RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().ErrorMsg = e.Message;
                RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().InitaDataAsync();
                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = errorView;
            }
            base.IsBusy = false;
        }

        private string ClearErrorCode(string errorcode)
        {
            string res = ConfigManager<Setting>.Config.clearErrorCode;
            if (res != null && errorcode.Contains(res))
            {
                errorcode = errorcode.Replace(res, "");
            }
            return errorcode;
        }
    }
}
