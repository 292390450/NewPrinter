// Knd.Printer.ReportPrinter.ViewModel.StateViewModel

using System.ComponentModel.Composition;
using Knd.Printer.CoreLibFrame45;

namespace Knd.Printer.ReportPrinter.ViewModel
{


    [Export]
    public class StateViewModel : ViewModelBase
    {
        private ReportPrinter _reportPrinter;

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
    }
}