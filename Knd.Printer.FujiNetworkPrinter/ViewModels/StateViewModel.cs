using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.FujiNetworkPrinter.Models;

namespace Knd.Printer.FujiNetworkPrinter.ViewModels
{
    [Export]
    public class StateViewModel : ViewModelBase
    {
        private FujiNetworkPrinter _fujiSerialPortPrinter;

    

        public FujiNetworkPrinter FujiSerialPortPrinter
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
           
        }
    }

}
