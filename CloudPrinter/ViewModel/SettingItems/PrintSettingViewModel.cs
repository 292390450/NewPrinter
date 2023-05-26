﻿// CloudPrinter.ViewModel.SettingItems.PrintSettingViewModel

using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing.Printing;
using System.Linq;
using CloudPrinter.Model;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;

namespace CloudPrinter.ViewModel.SettingItems;

[Export]
public class PrintSettingViewModel : ViewModelBase
{
    private Setting _setting;

    private ObservableCollection<string> _reportPrinterCollection;

    private ObservableCollection<SendFilmType> _sendFilmTypeCollection;

    private ObservableCollection<FilmPrinterType> _filmPrinterTypeCollection;

    private ObservableCollection<PrintFilmTimeModel> _printFilmTimeModels;
    public List<DicomType> PrintTypeEnum { get; set; } =    new List<DicomType>(Enum.GetValues(typeof(DicomType)).Cast<DicomType>());
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

    public ObservableCollection<string> ReportPrinterCollection
    {
        get
        {
            return _reportPrinterCollection;
        }
        set
        {
            _reportPrinterCollection = value;
            NotifyPropertyChanged("ReportPrinterCollection");
        }
    }

    public ObservableCollection<SendFilmType> SendFilmTypeCollection
    {
        get
        {
            return _sendFilmTypeCollection;
        }
        set
        {
            _sendFilmTypeCollection = value;
            NotifyPropertyChanged("SendFilmTypeCollection");
        }
    }

    public ObservableCollection<FilmPrinterType> FilmPrinterTypeCollection
    {
        get
        {
            return _filmPrinterTypeCollection;
        }
        set
        {
            _filmPrinterTypeCollection = value;
            NotifyPropertyChanged("FilmPrinterTypeCollection");
        }
    }

    public ObservableCollection<PrintFilmTimeModel> PrintFilmTimeModels
    {
        get
        {
            return _printFilmTimeModels;
        }
        set
        {
            _printFilmTimeModels = value;
            NotifyPropertyChanged("PrintFilmTimeModels");
        }
    }

    public RelayCommand SaveCommand { get; private set; }

    public RelayCommand AddPrintFilmCountCommand { get; private set; }

    public RelayCommand DelPrintFilmCountCommand { get; private set; }

    public override async void InitaDataAsync()
    {
        await ConfigManager<Setting>.LoadAsync();
        Setting = ConfigManager<Setting>.Config;
        if (ReportPrinterCollection == null)
        {
            ReportPrinterCollection = new ObservableCollection<string>();
            PrinterSettings.StringCollection systemPrinterNames = PrinterSettings.InstalledPrinters;
            RunTimeHost.MainDispatcher.Invoke(delegate
            {
                ReportPrinterCollection.AddRange(systemPrinterNames.Cast<string>());
            });
        }
        if ((Setting.ReportPrinterName == null || string.IsNullOrEmpty(Setting.ReportPrinterName)) && ReportPrinterCollection != null && ReportPrinterCollection.Count > 0)
        {
            Setting.ReportPrinterName = ReportPrinterCollection.FirstOrDefault();
            ConfigManager<Setting>.Config.ReportPrinterName = ReportPrinterCollection.FirstOrDefault();
            await ConfigManager<Setting>.Save();
        }
        PrintFilmTimeModels = Setting.printFilmTimeModels;
        if (SendFilmTypeCollection == null)
        {
            SendFilmTypeCollection = new ObservableCollection<SendFilmType>();
            SendFilmTypeCollection.Add(SendFilmType.SendByClient);
            SendFilmTypeCollection.Add(SendFilmType.sendByServer);
        }
        if (FilmPrinterTypeCollection == null)
        {
            FilmPrinterTypeCollection = new ObservableCollection<FilmPrinterType>();
            FilmPrinterTypeCollection.Add(FilmPrinterType.DicomPrinter);
            FilmPrinterTypeCollection.Add(FilmPrinterType.KndNewPrinter);
        }
        base.InitaDataAsync();
    }

    public override void InitCommand()
    {
        SaveCommand = new RelayCommand(async delegate
        {
            base.IsBusy = true;
            Setting.printFilmTimeModels = PrintFilmTimeModels;
            ConfigManager<Setting>.Config = Setting;
            await ConfigManager<Setting>.Save();
            ConfigManager<Setting>.Load();
            base.IsBusy = false;
        }, () => base.IsValid);
        AddPrintFilmCountCommand = new RelayCommand(delegate
        {
            if (PrintFilmTimeModels.Count < 4)
            {
                PrintFilmTimeModel item = new PrintFilmTimeModel
                {
                    PrintFilmCount = ConfigManager<Setting>.Config.printFilmTimeModels.LastOrDefault().PrintFilmCount + 1,
                    PrintFilmSize = "14INX17IN",
                    PrintFilmTime = 30
                };
                ConfigManager<Setting>.Config.printFilmTimeModels.Add(item);
                PrintFilmTimeModels = ConfigManager<Setting>.Config.printFilmTimeModels;
            }
        });
        DelPrintFilmCountCommand = new RelayCommand(delegate
        {
            if (PrintFilmTimeModels.Count > 1)
            {
                PrintFilmTimeModels.Remove(PrintFilmTimeModels.LastOrDefault());
                ConfigManager<Setting>.Config.printFilmTimeModels = PrintFilmTimeModels;
            }
        });
        base.InitCommand();
    }
}