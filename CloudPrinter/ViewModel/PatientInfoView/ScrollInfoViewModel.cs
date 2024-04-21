using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.Model;
using Knd.Printer.SecondApi;
using Ost.AutoPrinter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace CloudPrinter.ViewModel.PatientInfoView
{
    [Export]
    public class ScrollInfoViewModel : ViewModelBase
    {
        private Setting _setting;

        private ObservableCollection<PrintNotifyInfo> _printNotifyInfos;

        private Timer _timer;

        private Timer _timer_printNotifyInfos;

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

        public ObservableCollection<PrintNotifyInfo> PrintNotifyInfos
        {
            get
            {
                return _printNotifyInfos;
            }
            set
            {
                _printNotifyInfos = value;
                NotifyPropertyChanged("PrintNotifyInfos");
            }
        }

        public override void InitaDataAsync()
        {
            if (_timer == null)
            {
                _timer = new Timer((ConfigManager<Setting>.Config.RefreshTime == 0) ? 10000 : (ConfigManager<Setting>.Config.RefreshTime * 1000));
                _timer.Elapsed += _timer_Elapsed;
            }
            if (_timer_printNotifyInfos == null)
            {
                _timer_printNotifyInfos = new Timer((ConfigManager<Setting>.Config.ScrollTime == 0) ? 10000 : (ConfigManager<Setting>.Config.ScrollTime * 1000));
                _timer_printNotifyInfos.Elapsed += _timer_printNotifyInfos_Elapsed;
            }
            _timer_printNotifyInfos.Start();
            _timer.Start();
            GetPinfo();
            Setting = ConfigManager<Setting>.Config;
            base.InitaDataAsync();
        }

        private void _timer_printNotifyInfos_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunTimeHost.MainDispatcher.BeginInvoke((Action)delegate
            {
                if (PrintNotifyInfos != null && PrintNotifyInfos.Count > 1)
                {
                    PrintNotifyInfos.RemoveAt(0);
                }
                else
                {
                    GetPinfo();
                }
            });
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (PrintNotifyInfos == null || PrintNotifyInfos.Count <= 5)
            {
                GetPinfo();
            }
        }

        public void GetPinfo()
        {
            //ConfigManager<Setting>.Load();
            DateTime day = DateTime.Now.AddDays(1 - ConfigManager<Setting>.Config.ShowInfoDay);
            IList<PrintNotifyInfo> res = null;
            try
            {
                res = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().GetInfoEntities(new DateTime(day.Year, day.Month, day.Day, 0, 0, 0));
                res = res.Where(x => x.InfoTime >= day.Date).ToList();
            }
            catch (Exception exc)
            {
                LogManager.AddLog("获取可打印列表出错：" + exc.Message);
            }
            if (res == null)
            {
                return;
            }
            if (PrintNotifyInfos == null)
            {
                PrintNotifyInfos = new ObservableCollection<PrintNotifyInfo>();
            }
            RunTimeHost.MainDispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                PrintNotifyInfos.Clear();
                PrintNotifyInfos.AddRange(res);
                try
                {
                    if (PrintNotifyInfos.Count > 0 && !string.IsNullOrEmpty(ConfigManager<Setting>.Config.NameReplace))
                    {
                        string nameReplace = ConfigManager<Setting>.Config.NameReplace;
                        if (nameReplace.Contains(","))
                        {
                            string[] array = nameReplace.Split(',');
                            string[] array2 = array;
                            foreach (string value in array2)
                            {
                                int num = Convert.ToInt16(value);
                                if (num - 1 > 0)
                                {
                                    foreach (PrintNotifyInfo current in PrintNotifyInfos)
                                    {
                                        if (current.Name != null && current.Name.Length >= num)
                                        {
                                            char[] array3 = current.Name.ToCharArray();
                                            array3[num - 1] = '*';
                                            current.Name = new string(array3);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int num2 = Convert.ToInt16(nameReplace);
                            if (num2 > 0)
                            {
                                foreach (PrintNotifyInfo current2 in PrintNotifyInfos)
                                {
                                    if (current2.Name != null && current2.Name.Length >= num2)
                                    {
                                        char[] array4 = current2.Name.ToCharArray();
                                        array4[num2 - 1] = '*';
                                        current2.Name = new string(array4);
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddLog("提示屏幕中，替换姓名出错" + ex.Message);
                }
            });
        }
    }
}
