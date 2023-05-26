using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CloudPrinter.View.PatientInfoView;

namespace CloudPrinter.ViewModel
{

    [Export]
    public class TopWindowViewModel : ViewModelBase
    {
        private Setting _setting;

        private UserControl _infoControl;

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

        public UserControl InfoControl
        {
            get
            {
                return _infoControl;
            }
            set
            {
                _infoControl = value;
                NotifyPropertyChanged("InfoControl");
            }
        }

        public override void InitaDataAsync()
        {
            InfoControl = RunTimeHost.MEFContainer.GetExportedValue<ScrollInfoView>();
            Setting = ConfigManager<Setting>.Config;
            base.InitaDataAsync();
        }
    }
}
