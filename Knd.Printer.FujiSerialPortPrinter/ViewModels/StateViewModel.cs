using Knd.Printer.Core;
using Knd.Printer.FujiSerialPortPrinter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.ConfigHelper;

namespace Knd.Printer.FujiSerialPortPrinter.ViewModels
{
    [Export]
    public class StateViewModel : ViewModelBase
    {
        private FujiSerialPortPrinterr _fujiSerialPortPrinter;

        private ObservableCollection<FilmBoxModel> _filmBoxs;

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

        public FujiSerialPortPrinterr FujiSerialPortPrinter
        {
            get
            {
                return _fujiSerialPortPrinter;
            }
            set
            {
                _fujiSerialPortPrinter = value;
                NotifyPropertyChanged("FujiSerialPortPrinter");
            }
        }

        public void Inita()
        {
            if (FilmBoxs == null)
            {
                FilmBoxs = new ObservableCollection<FilmBoxModel>();
            }
            FilmBoxs.Clear();
            RunTimeHost.MainDispatcher.Invoke(delegate
            {
                FilmBoxs.AddRange(ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs);
            });
        }
    }

}
