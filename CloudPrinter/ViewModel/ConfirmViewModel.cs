using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using Ost.AutoPrinter.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudPrinter.View;
using Knd.Printer.Core;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class ConfirmViewModel : ViewModelBase
    {
        private IList<PrintTaskInfo> _taskInfos;

        private string _name;

        private string _accno;

        private int _reportCount;

        private int _filmCount;

        private InputPara _currentInputPara;

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

        public RelayCommand ReturnCommand { get; private set; }

        public RelayCommand<string> PrintCommand { get; private set; }

        public override void InitaDataAsync()
        {
            base.InitaDataAsync();
        }

        public override void InitCommand()
        {
            ReturnCommand = new RelayCommand(delegate
            {
                MainViewModel exportedValue = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>();
                exportedValue.CurrentControl = exportedValue?.WaitCardView;
            });
            PrintCommand = new RelayCommand<string>(delegate (string type)
            {
                if (type == "3")
                {
                    LogManager.AddLog("选择了只获取电子胶片");
                    RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<OnlyCloudPrintView>();
                    RunTimeHost.MEFContainer.GetExportedValue<OnlyCloudPrintViewModel>().InitSetTasks(_taskInfos, (PrintType)int.Parse(type), _currentInputPara);
                }
                else
                {
                    RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<PrintingView>();
                    RunTimeHost.MEFContainer.GetExportedValue<PrintingViewModel>().InitSetTasks(_taskInfos, (PrintType)int.Parse(type), _currentInputPara);
                }
            });
            base.InitCommand();
        }

        public void Init(IList<PrintTaskInfo> taskInfos, InputPara inputPara)
        {
            _taskInfos = taskInfos;
            _currentInputPara = inputPara;
            Name = _taskInfos.FirstOrDefault()?.PatientName;
            Accno = _taskInfos.FirstOrDefault()?.AccNo;
            ReportCount = 0;
            FilmCount = 0;
            foreach (PrintTaskInfo printTaskInfo in _taskInfos)
            {
                ReportCount += printTaskInfo.CanPrintReportCount;
                FilmCount += printTaskInfo.CanPintFilmCount;
            }
        }
    }
}
