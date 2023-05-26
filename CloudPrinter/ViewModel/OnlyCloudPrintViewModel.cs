using Knd.Printer.Common.Helper;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using Ost.AutoPrinter.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Media.Imaging;
using System.Windows;
using CloudPrinter.View;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class OnlyCloudPrintViewModel : ViewModelBase
    {
        private string _accno;

        private string _name;

        private int _reportCount;

        private int _filmCount;

        private string _time;

        private Timer _timer;

        private BitmapSource _qrImage;

        private InputPara _currentInputPara;

        public string Time
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

        public RelayCommand ReturenWaiCommand { get; private set; }

        public override void InitCommand()
        {
            ReturenWaiCommand = new RelayCommand(delegate
            {
                _timer.Stop();
                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().WaitCardView;
            });
            base.InitCommand();
        }

        public override void InitaDataAsync()
        {
            base.InitaDataAsync();
        }

        public async void InitSetTasks(IList<PrintTaskInfo> taskInfos, PrintType printType, InputPara inputPara)
        {
            OnlyCloudPrintView view = RunTimeHost.MEFContainer.GetExportedValue<OnlyCloudPrintView>();
            VisualStateManager.GoToElementState(view, "GetState", useTransitions: false);
            _currentInputPara = inputPara;
            Accno = taskInfos.FirstOrDefault()?.AccNo;
            Name = taskInfos.FirstOrDefault()?.PatientName;
            ReportCount = 0;
            FilmCount = 0;
            foreach (PrintTaskInfo printTaskInfo in taskInfos)
            {
                ReportCount += printTaskInfo.CanPrintReportCount;
                FilmCount += printTaskInfo.CanPintFilmCount;
            }
            QrImage = QrCodeHelper.GetBitmapImage(await GenerateQrImage(Accno, Name, FilmCount, ReportCount, ConfigManager<Setting>.Config.UploadClientId, ConfigManager<Setting>.Config.ClientId, RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().pay_key));
            if (_timer == null)
            {
                _timer = new Timer(1000.0);
                _timer.Elapsed += _timer_Elapsed;
            }
            if (!ConfigManager<Setting>.Config.IsWaitState)
            {
                Time = 60.ToString() ?? "";
                _timer.Start();
            }
            else
            {
                Time = "";
            }
        }

        public void SetState(MqMessageModel mqMsg)
        {
            try
            {
                if (mqMsg.accno != Accno || DateTime.Now - Convert.ToDateTime(mqMsg.timestamp) > TimeSpan.FromMinutes(2.0))
                {
                    return;
                }
            }
            catch (Exception e)
            {
                LogManager.AddLog(e);
                return;
            }
            OnlyCloudPrintView view = RunTimeHost.MEFContainer.GetExportedValue<OnlyCloudPrintView>();
            switch (mqMsg.ewmCallbackType)
            {
                case 1:
                    VisualStateManager.GoToElementState(view, "Sweeped", useTransitions: false);
                    if (!ConfigManager<Setting>.Config.IsWaitState)
                    {
                        Time = 60.ToString() ?? "";
                        _timer.Start();
                    }
                    break;
                case 2:
                    VisualStateManager.GoToElementState(view, "PayingState", useTransitions: false);
                    if (!ConfigManager<Setting>.Config.IsWaitState)
                    {
                        Time = 60.ToString() ?? "";
                        _timer.Start();
                    }
                    break;
                case 3:
                    VisualStateManager.GoToElementState(view, "PaySucess", useTransitions: false);
                    Time = 20.ToString() ?? "";
                    _timer.Start();
                    break;
                case 4:
                    VisualStateManager.GoToElementState(view, "Fail", useTransitions: false);
                    Time = 20.ToString() ?? "";
                    _timer.Start();
                    break;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (int.Parse(Time) <= 0)
            {
                _timer.Stop();
                ReturenWaiCommand.Execute(null);
            }
            Time = (int.Parse(Time) - 1).ToString() ?? "";
        }

        private Task<Bitmap> GenerateQrImage(string accno, string patientName, int filmNum, int reportNum, string uploadClientId, string clientId, string payKey)
        {
            return Task.Run(delegate
            {
                string input = RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().hosptail_id + accno + patientName + filmNum + reportNum + uploadClientId + clientId + 1 + payKey;
                string text = string.Empty;
                using (MD5 md5Hash = MD5.Create())
                {
                    text = MD5Helper.GetMd5Hash(md5Hash, input);
                }
                string arg = HttpUtility.UrlEncode(patientName);
                string content = RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().pay_url + $"?hsid={RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().hosptail_id}&accno={Accno}&patientName={arg}" + $"&filmNum={filmNum}&reportNum={reportNum}&uploadClientId={uploadClientId}&clientId={clientId}" + "&ewmType=1&md5=" + text;
                return QrCodeHelper.GeneratCodeImage(content, 150, 150);
            });
        }
    }
}
