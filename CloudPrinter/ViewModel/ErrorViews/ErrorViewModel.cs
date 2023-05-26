using Knd.Printer.CoreLibFrame45.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GalaSoft.MvvmLight.Messaging;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;

namespace CloudPrinter.ViewModel.ErrorViews
{
    [Export]
    public class ErrorViewModel : ViewModelBase
    {
        private int _time;

        private Timer _timer;

        private string _errorMsg;

        public int Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

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

        public RelayCommand ReturenWaiCommand { get; private set; }

        public override void InitCommand()
        {
            ReturenWaiCommand = new RelayCommand(delegate
            {
                _timer.Stop();
                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().WaitCardView;
                Messenger.Default.Send<object>(null, "StartReaderCard");
            });
            base.InitCommand();
        }

        public override void InitaDataAsync()
        {
            Time = 5;
            if (_timer == null)
            {
                _timer = new Timer(1000.0);
                _timer.Elapsed += _timer_Elapsed;
            }
            _timer.Start();
            base.InitaDataAsync();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Time <= 0)
            {
                _timer.Stop();
                ReturenWaiCommand.Execute(null);
            }
            Time--;
        }
    }
}
