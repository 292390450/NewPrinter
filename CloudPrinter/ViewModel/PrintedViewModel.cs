using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Windows;
using CloudPrinter.View;
using GalaSoft.MvvmLight.Messaging;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class PrintedViewModel : ViewModelBase
    {
        private string _accno;

        private string _name;

        private int _reportCount;

        private int _filmCount;

        private int _printedReportCount;

        private int _printedFilmCount;

        private int _time;

        private Timer _timer;

        private BitmapSource _qrImage;

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

        public BitmapSource QrImage
        {
            get
            {
                return _qrImage;
            }
            set
            {
                _qrImage = value;
                NotifyPropertyChanged("QrImage");
            }
        }

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

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public int ReportCount
        {
            get
            {
                return _reportCount;
            }
            set
            {
                _reportCount = value;
                NotifyPropertyChanged("ReportCount");
            }
        }

        public int FilmCount
        {
            get
            {
                return _filmCount;
            }
            set
            {
                _filmCount = value;
                NotifyPropertyChanged("FilmCount");
            }
        }

        public int PrintedReportCount
        {
            get
            {
                return _printedReportCount;
            }
            set
            {
                _printedReportCount = value;
                NotifyPropertyChanged("PrintedReportCount");
            }
        }

        public int PrintedFilmCount
        {
            get
            {
                return _printedFilmCount;
            }
            set
            {
                _printedFilmCount = value;
                NotifyPropertyChanged("PrintedFilmCount");
            }
        }

        public RelayCommand ReturenWaiCommand { get; private set; }

        public override void InitCommand()
        {
            ReturenWaiCommand = new RelayCommand(delegate
            {
                _timer.Stop();
                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().WaitCardView;
                Messenger.Default.Send("WaitingPrint", "LightControl");
            });
            base.InitCommand();
        }

        public override void InitaDataAsync()
        {
            if (_timer == null)
            {
                _timer = new Timer(1000.0);
                _timer.Elapsed += _timer_Elapsed;
            }
            base.InitaDataAsync();
        }

        public void Init(string name, string accno, int filmCount, int reportCount, int printFilmCount, int printReportCount, PrintType printType, BitmapSource bit)
        {
            PrintedView view = RunTimeHost.MEFContainer.GetExportedValue<PrintedView>();
            switch (printType)
            {
                case PrintType.ReportsAndFilms:
                    VisualStateManager.GoToElementState(view, "ReportAndFilm", useTransitions: false);
                    break;
                case PrintType.ReportsAndFilmsAndCloud:
                    VisualStateManager.GoToElementState(view, "ReportAndFilmAndCloud", useTransitions: false);
                    break;
            }
            Name = name;
            Accno = accno;
            FilmCount = filmCount;
            ReportCount = reportCount;
            PrintedFilmCount = printFilmCount;
            PrintedReportCount = printReportCount;
            QrImage = bit;
            Time = ConfigManager<Setting>.Config.PrintedReturnTime;
            _timer?.Start();
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
