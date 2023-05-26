// CloudPrinter.ViewModel.SettingItems.KeyboardInputViewModel

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using CloudPrinter.View;
using CloudPrinter.View.ErrorViews;
using CloudPrinter.ViewModel.ErrorViews;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;
using Knd.Printer.SecondApi;
using Ost.AutoPrinter.Api;

namespace CloudPrinter.ViewModel.SettingItems;

[Export]
public class KeyboardInputViewModel : ViewModelBase
{
    private string _accno;

    private string _keyboredStr;

    private ObservableCollection<string> _keyCollection;

    public string Accno
    {
        get
        {
            return _accno;
        }
        set
        {
            _accno = value;
            NotifyPropertyChanged("Accno");
        }
    }

    public string KeyboredStr
    {
        get
        {
            return _keyboredStr;
        }
        set
        {
            _keyboredStr = value;
            NotifyPropertyChanged("KeyboredStr");
        }
    }

    public ObservableCollection<string> KeyCollection
    {
        get
        {
            return _keyCollection;
        }
        set
        {
            _keyCollection = value;
            NotifyPropertyChanged("KeyCollection");
        }
    }

    public RelayCommand<string> KeyDownCommand { get; set; }

    public RelayCommand RemoveAccnoLast { get; set; }

    public RelayCommand AchievePrintTasksByAccno { get; set; }

    public RelayCommand SaveCommand { get; set; }

    public override void InitCommand()
    {
        KeyDownCommand = new RelayCommand<string>(delegate (string accno)
        {
            Accno += accno;
        });
        RemoveAccnoLast = new RelayCommand(delegate
        {
            if (Accno != null && Accno.Length > 0)
            {
                Accno = Accno.Remove(Accno.Length - 1);
            }
        });
        SaveCommand = new RelayCommand(delegate
        {
            ConfigManager<Setting>.Config.KeyboredStr = KeyboredStr;
            ConfigManager<Setting>.Save();
            if (KeyCollection == null)
            {
                KeyCollection = new ObservableCollection<string>();
            }
            if (KeyboredStr != null && KeyboredStr.Length > 0)
            {
                KeyCollection.Clear();
                for (int i = 0; i < KeyboredStr.Length; i++)
                {
                    KeyCollection.Add(KeyboredStr.ElementAt(i).ToString());
                }
            }
        });
        AchievePrintTasksByAccno = new RelayCommand(async delegate
        {
            LogManager.AddLog("手动输入检查号：" + Accno);
            if (Accno != null && !string.IsNullOrEmpty(Accno))
            {
                try
                {
                    if (!ConfigManager<Setting>.Config.IsOpenSelfService)
                    {
                        throw new Exception("终端配置未启用自助服务");
                    }
                    IList<PrintTaskInfo> res = await LoadPatientInfo(Accno, "01", null);
                    if (ConfigManager<Setting>.Config.IsOpenColud && RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().is_open_pay == 1)
                    {
                        LogManager.AddLog("跳转到云影像确认界面");
                        RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<ConfirmView>();
                        RunTimeHost.MEFContainer.GetExportedValue<ConfirmViewModel>().Init(res, new InputPara
                        {
                            EqpCode = "01",
                            ext = "",
                            Num = Accno
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
                            Num = Accno
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
                Accno = null;
            }
        });
    }

    private Task<IList<PrintTaskInfo>> LoadPatientInfo(string num, string eqp, string msg)
    {
        return RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().GetPrintTaskInfoAsync(num, eqp, msg);
    }

    public override void InitaDataAsync()
    {
        Accno = null;
        if (KeyboredStr == null)
        {
            ConfigManager<Setting>.Load();
            KeyboredStr = ConfigManager<Setting>.Config.KeyboredStr;
        }
        if (KeyCollection != null)
        {
            return;
        }
        KeyCollection = new ObservableCollection<string>();
        if (KeyboredStr != null && KeyboredStr.Length > 0)
        {
            for (int i = 0; i < KeyboredStr.Length; i++)
            {
                KeyCollection.Add(KeyboredStr.ElementAt(i).ToString());
            }
        }
    }
}