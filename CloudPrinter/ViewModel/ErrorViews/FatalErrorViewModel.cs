using Knd.Printer.CoreLibFrame45.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Knd.Printer.CoreLibFrame45;

namespace CloudPrinter.ViewModel.ErrorViews
{
    [Export]
    public class FatalErrorViewModel : ViewModelBase
    {
        private string _errorMsg;

        public string ErrorMsg
        {
            get
            {
                return _errorMsg;
            }
            set
            {
                _errorMsg = value;
                NotifyPropertyChanged("ErrorMsg");
            }
        }

        public RelayCommand ShutDownCommand { get; set; }

        public override void InitCommand()
        {
            ShutDownCommand = new RelayCommand(delegate
            {
                Application.Current.Shutdown();
            });
        }
    }
}
