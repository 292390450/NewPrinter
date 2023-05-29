// Knd.Printer.ReportPrinter.ViewModel.SettingViewModel

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing.Printing;
using System.Linq;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.ReportPrinter.Config;

namespace Knd.Printer.ReportPrinter.ViewModel;

[Export]
public class SettingViewModel : ViewModelBase
{
    ReportPrinter _reportPrinter;

    private string _printName;

    private int _refreshTime;

    private ObservableCollection<string> _printerCollection;

    public ReportPrinter ReportPrinter
    {
        get
        {
            return _reportPrinter;
        }
        set
        {
            _reportPrinter = value;
            NotifyPropertyChanged("ReportPrinter");
        }
    }

    public string PrintName
    {
        get
        {
            return _printName;
        }
        set
        {
            _printName = value;
            NotifyPropertyChanged("PrintName");
        }
    }

    public int RefreshTime
    {
        get
        {
            return _refreshTime;
        }
        set
        {
            _refreshTime = value;
            NotifyPropertyChanged("RefreshTime");
        }
    }

    public ObservableCollection<string> PrinterCollection
    {
        get
        {
            return _printerCollection;
        }
        set
        {
            _printerCollection = value;
            NotifyPropertyChanged("PrinterCollection");
        }
    }

    public RelayCommand SaveCommand { get; private set; }

    public async void SettingViewModelInit()
    {
        await ConfigManager<ReportPrinterConfig>.LoadAsync();
        RefreshTime = ConfigManager<ReportPrinterConfig>.Config.RefreshTime;
        PrintName = ConfigManager<ReportPrinterConfig>.Config.Name;
        if (PrinterCollection == null)
        {
            PrinterCollection = new ObservableCollection<string>();
            PrinterSettings.StringCollection systemPrinterNames = PrinterSettings.InstalledPrinters;
            RunTimeHost.MainDispatcher.Invoke(delegate
            {
                PrinterCollection.AddRange(systemPrinterNames.Cast<string>());
            });
        }
    }

    public override void InitCommand()
    {
        SaveCommand = new RelayCommand(delegate
        {
            ConfigManager<ReportPrinterConfig>.Config.RefreshTime = RefreshTime;
            ConfigManager<ReportPrinterConfig>.Config.Name = PrintName;
        }, () => base.IsValid);
        base.InitCommand();
    }
}