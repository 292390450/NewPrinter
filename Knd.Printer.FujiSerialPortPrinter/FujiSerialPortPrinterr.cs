using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using Knd.Printer.Abstract;
using Knd.Printer.Common.Helper;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.FujiSerialPortPrinter.Models;
using Knd.Printer.FujiSerialPortPrinter.ViewModels;
using Knd.Printer.FujiSerialPortPrinter.Views;
using Knd.Printer.Model;
using Newtonsoft.Json;
using Ost.AutoPrinter.Api;

namespace Knd.Printer.FujiSerialPortPrinter
{
    [Export(typeof(IBaseDevice))]
    public class FujiSerialPortPrinterr : PrinterDevice, INotifyPropertyChanged
    {
        private Timer _timer;

        private Assembly _currentAssembly;

        private string _stateStr;

        private FilmBoxModel[] _filmBoxs;

        private PrinterState _state;

        private bool _isOpen;

        private bool _isOpenSerialProt;

        private string _comNames;

        private int _refreshTime;

        private string basePath;

        public string StateStr
        {
            get
            {
                return _stateStr;
            }
            set
            {
                _stateStr = value;
                NotifyPropertyChanged("StateStr");
            }
        }

        public FilmBoxModel[] FilmBoxs
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

        public override PrinterState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                NotifyPropertyChanged("State");
            }
        }

        public bool IsOpenStateView
        {
            get
            {
                return _isOpen;
            }
            set
            {
                _isOpen = value;
                NotifyPropertyChanged("IsOpenStateView");
            }
        }

        public bool IsOpenSerialProt
        {
            get
            {
                return _isOpenSerialProt;
            }
            set
            {
                _isOpenSerialProt = value;
                NotifyPropertyChanged("IsOpenSerialProt");
            }
        }

        public string ComNames
        {
            get
            {
                return _comNames;
            }
            set
            {
                _comNames = value;
                NotifyPropertyChanged("ComNames");
            }
        }

        public int RefreshTime
        {
            get
            {
                return _refreshTime;
            }
            set
            {
                _refreshTime = value;
                NotifyPropertyChanged("RefreshTime");
            }
        }

        public override PrinterType Type { get; set; }

        public override string Name { get; set; } = "KND3500SerialPort相机";


        [Import(typeof(SettingView))]
        public override ContentControl SettingView { get; set; }

        [Import(typeof(StateView))]
        public override ContentControl StateView { get; set; }

        public override Action Selected { get; set; }

        private FujiErrorCode fujiErrorCode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override void Check()
        {
        }

        public override void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer(RefreshTime);
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            string[] printMessenger = new string[3];
            string filmInfo = "";
            if (ConfigManager<FujiPrinterConfigModel>.Config.IsOpen && FilmBoxs != null)
            {
                for (int i = 0; i < FilmBoxs.Length; i++)
                {
                    filmInfo = filmInfo + FilmBoxs[i].Size + ",film." + i + "," + FilmBoxs[i].Count + "-";
                }
            }
            printMessenger[0] = filmInfo;
            if (IsOpenSerialProt)
            {
                CheckFujiState();
            }
            switch (State)
            {
                case PrinterState.就绪:
                    printMessenger[1] = "就绪";
                    break;
                case PrinterState.打印中:
                    printMessenger[1] = "打印中";
                    break;
                case PrinterState.错误:
                    printMessenger[1] = "错误";
                    printMessenger[2] = "错误";
                    break;
                case PrinterState.警告:
                    printMessenger[1] = "警告";
                    printMessenger[2] = "警告";
                    break;
                case PrinterState.未知:
                    printMessenger[1] = "未知";
                    printMessenger[2] = "未知";
                    break;
            }
            Messenger.Default.Send(printMessenger, "SendClientInfoToServer");
            _timer.Start();
        }

        public override void Stop()
        {
        }

        public override async void ResetFilmCountAsync(string num)
        {
            string resetBox = num[2].ToString() ?? "";
            try
            {
                int index = Convert.ToInt32(resetBox);
                if (FilmBoxs.Length >= index)
                {
                    FilmBoxs[index - 1].Count = Convert.ToInt32(num.Substring(3));
                }
            }
            catch (Exception ex)
            {
                LogManager.AddLog("重置数量出错" + ex.Message);
            }
            ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs = FilmBoxs;
            await ConfigManager<FujiPrinterConfigModel>.Save();
            RunTimeHost.MainDispatcher.Invoke(delegate
            {
                ((StateViewModel)StateView.DataContext).Inita();
            });
        }

        public override void BeginTaskPrint(IList<PrintTaskInfo> taskInfos)
        {
            State = PrinterState.打印中;
            StateStr = "打印中";
            if (!ConfigManager<FujiPrinterConfigModel>.Config.IsOpen)
            {
                return;
            }
            IList<FilmDocumentInfo> films = new List<FilmDocumentInfo>();
            foreach (PrintTaskInfo item in taskInfos)
            {
                films.AddRange(item.Films);
            }
            Dictionary<string, int> filmBoxsDictionary = new Dictionary<string, int>();
            ConfigManager<FujiPrinterConfigModel>.Load();
            FilmBoxs = ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs;
            FilmBoxModel[] filmBoxs = FilmBoxs;
            foreach (FilmBoxModel box in filmBoxs)
            {
                if (filmBoxsDictionary.ContainsKey(box.Size))
                {
                    int count = filmBoxsDictionary[box.Size];
                    filmBoxsDictionary[box.Size] = count + box.Count;
                }
                else
                {
                    filmBoxsDictionary.Add(box.Size, box.Count);
                }
            }
            foreach (FilmDocumentInfo item2 in films)
            {
                if (!filmBoxsDictionary.ContainsKey(item2.Size))
                {
                    throw new Exception("该任务中存在" + item2.Size + "尺寸的胶片,但是打印机中没有该尺寸胶片，请到其它机器打印");
                }
                if (films.Count > filmBoxsDictionary[item2.Size])
                {
                    AudioHelper.Speak("胶片数量不足，请到其它机器打印");
                    throw new Exception(item2.Size + "胶片数量少于" + films.Count + ",请到其它机器打印");
                }
            }
        }

        public override void EndTaskPrint(IList<PrintTaskInfo> taskInfos, bool isSuccess)
        {
            State = PrinterState.就绪;
            StateStr = "就绪";
        }

        public override void BeginDocumentPrint(AbstractDocumentInfo documentInfo)
        {
        }

        public override async void EndDocumentPrint(AbstractDocumentInfo documentInfo, bool isSuccess)
        {
            if (!isSuccess || !(documentInfo is FilmDocumentInfo) || !ConfigManager<FujiPrinterConfigModel>.Config.IsOpen)
            {
                return;
            }
            int index = -1;
            ConfigManager<FujiPrinterConfigModel>.Load();
            FilmBoxs = ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs;
            for (int i = 0; i < FilmBoxs.Length; i++)
            {
                if (index >= 0 && FilmBoxs[i].Size == documentInfo.Size)
                {
                    if (FilmBoxs[index].Count > FilmBoxs[i].Count && FilmBoxs[i].Count > 0)
                    {
                        index = i;
                    }
                }
                else if (FilmBoxs[i].Size == documentInfo.Size)
                {
                    index = i;
                }
            }
            FilmBoxs[index].Count = FilmBoxs[index].Count - 1;
            ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs = FilmBoxs;
            await ConfigManager<FujiPrinterConfigModel>.Save();
            ConfigManager<FujiPrinterConfigModel>.Load();
            RunTimeHost.MainDispatcher.Invoke(delegate
            {
                ((StateViewModel)StateView.DataContext).Inita();
            });
        }

        public void CheckFujiState()
        {
            try
            {
                if (fujiErrorCode != null)
                {
                    fujiErrorCode.SendData();
                    FujiStateCode fujiStateCodeNow = fujiErrorCode.fujiStateCodeNow;
                    if (fujiStateCodeNow.Code != "")
                    {
                        ChangeView(fujiStateCodeNow);
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send(ex, "PrinterCommunicationError");
            }
        }

        private void ChangeView(FujiStateCode fujiStateCodeNow)
        {
            switch (fujiStateCodeNow.PState)
            {
                case 1:
                    StateStr = "打印中";
                    State = PrinterState.打印中;
                    break;
                case 2:
                    StateStr = "警告";
                    State = PrinterState.警告;
                    Messenger.Default.Send(fujiErrorCode.fujiStateCodeNow.Msg, "FujiPrinterCommunicationError");
                    break;
                case 3:
                    StateStr = "错误";
                    State = PrinterState.错误;
                    Messenger.Default.Send(fujiErrorCode.fujiStateCodeNow.Msg, "FujiPrinterCommunicationError");
                    break;
                case 4:
                    StateStr = "就绪";
                    State = PrinterState.就绪;
                    break;
            }
        }

        public override async void Initial()
        {
            _currentAssembly = GetType().Assembly;
            basePath = _currentAssembly.Location.Replace(_currentAssembly.ManifestModule.Name, "");
            string configPath = basePath + "Config\\" + GetType().Name + ".json";
            try
            {
                ConfigManager<FujiPrinterConfigModel>.Init(configPath, ConfigType.Json, isLog: true);
                if (!File.Exists(configPath))
                {
                    FujiPrinterConfigModel fujiPrinterConfigModel = new FujiPrinterConfigModel();
                    fujiPrinterConfigModel.FilmBoxs = new FilmBoxModel[2]
                    {
                    new FilmBoxModel
                    {
                        Size = "14INX17IN"
                    },
                    new FilmBoxModel
                    {
                        Size = "10INX14IN"
                    }
                    };
                    fujiPrinterConfigModel.IsOpen = false;
                    fujiPrinterConfigModel.IsOpenSerialPort = false;
                    fujiPrinterConfigModel.ComName = "COM1";
                    fujiPrinterConfigModel.RefreshTime = 100;
                    ConfigManager<FujiPrinterConfigModel>.Config = fujiPrinterConfigModel;
                    ComNames = "COM1";
                    RefreshTime = 100;
                    IsOpenStateView = false;
                    IsOpenSerialProt = false;
                    await ConfigManager<FujiPrinterConfigModel>.Save();
                }
                else
                {
                    ConfigManager<FujiPrinterConfigModel>.Load();
                    IsOpenStateView = ConfigManager<FujiPrinterConfigModel>.Config.IsOpen;
                    FilmBoxs = ConfigManager<FujiPrinterConfigModel>.Config.FilmBoxs;
                    ComNames = ConfigManager<FujiPrinterConfigModel>.Config.ComName;
                    IsOpenSerialProt = ConfigManager<FujiPrinterConfigModel>.Config.IsOpenSerialPort;
                    RefreshTime = ConfigManager<FujiPrinterConfigModel>.Config.RefreshTime;
                }
                if (IsOpenSerialProt && fujiErrorCode == null)
                {
                    fujiErrorCode = new FujiErrorCode(ComNames);
                    fujiErrorCode.openCom();
                    CheckFujiState();
                }
                StateViewModel viewmodel = RunTimeHost.MEFContainer.GetExportedValue<StateViewModel>();
                viewmodel.Inita();
                viewmodel.FujiSerialPortPrinter = this;
                SettingViewModel settingViewModel = RunTimeHost.MEFContainer.GetExportedValue<SettingViewModel>();
                settingViewModel.Inita();
                settingViewModel.FujiSerialPortPrinterr = this;
                Selected = delegate
                {
                    RunTimeHost.MEFContainer.GetExportedValue<SettingViewModel>().Inita();
                };
            }
            catch (Exception ex2)
            {
                Exception ex = ex2;
                LogManager.AddLog("初始化FujiSerialPortPrinterr出错：" + ex.Message);
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class FujiErrorCode
    {
        public static Dictionary<string, FujiStateCode> fujiStateCodeDic = new Dictionary<string, FujiStateCode>();

        public static SerialPort _port;

        public static string Com = string.Empty;

        public FujiStateCode fujiStateCodeNow;

        private static byte[] auchCRCHi = new byte[256]
        {
        0, 193, 129, 64, 1, 192, 128, 65, 1, 192,
        128, 65, 0, 193, 129, 64, 1, 192, 128, 65,
        0, 193, 129, 64, 0, 193, 129, 64, 1, 192,
        128, 65, 1, 192, 128, 65, 0, 193, 129, 64,
        0, 193, 129, 64, 1, 192, 128, 65, 0, 193,
        129, 64, 1, 192, 128, 65, 1, 192, 128, 65,
        0, 193, 129, 64, 1, 192, 128, 65, 0, 193,
        129, 64, 0, 193, 129, 64, 1, 192, 128, 65,
        0, 193, 129, 64, 1, 192, 128, 65, 1, 192,
        128, 65, 0, 193, 129, 64, 0, 193, 129, 64,
        1, 192, 128, 65, 1, 192, 128, 65, 0, 193,
        129, 64, 1, 192, 128, 65, 0, 193, 129, 64,
        0, 193, 129, 64, 1, 192, 128, 65, 1, 192,
        128, 65, 0, 193, 129, 64, 0, 193, 129, 64,
        1, 192, 128, 65, 0, 193, 129, 64, 1, 192,
        128, 65, 1, 192, 128, 65, 0, 193, 129, 64,
        0, 193, 129, 64, 1, 192, 128, 65, 1, 192,
        128, 65, 0, 193, 129, 64, 1, 192, 128, 65,
        0, 193, 129, 64, 0, 193, 129, 64, 1, 192,
        128, 65, 0, 193, 129, 64, 1, 192, 128, 65,
        1, 192, 128, 65, 0, 193, 129, 64, 1, 192,
        128, 65, 0, 193, 129, 64, 0, 193, 129, 64,
        1, 192, 128, 65, 1, 192, 128, 65, 0, 193,
        129, 64, 0, 193, 129, 64, 1, 192, 128, 65,
        0, 193, 129, 64, 1, 192, 128, 65, 1, 192,
        128, 65, 0, 193, 129, 64
        };

        private static byte[] auchCRCLo = new byte[256]
        {
        0, 192, 193, 1, 195, 3, 2, 194, 198, 6,
        7, 199, 5, 197, 196, 4, 204, 12, 13, 205,
        15, 207, 206, 14, 10, 202, 203, 11, 201, 9,
        8, 200, 216, 24, 25, 217, 27, 219, 218, 26,
        30, 222, 223, 31, 221, 29, 28, 220, 20, 212,
        213, 21, 215, 23, 22, 214, 210, 18, 19, 211,
        17, 209, 208, 16, 240, 48, 49, 241, 51, 243,
        242, 50, 54, 246, 247, 55, 245, 53, 52, 244,
        60, 252, 253, 61, 255, 63, 62, 254, 250, 58,
        59, 251, 57, 249, 248, 56, 40, 232, 233, 41,
        235, 43, 42, 234, 238, 46, 47, 239, 45, 237,
        236, 44, 228, 36, 37, 229, 39, 231, 230, 38,
        34, 226, 227, 35, 225, 33, 32, 224, 160, 96,
        97, 161, 99, 163, 162, 98, 102, 166, 167, 103,
        165, 101, 100, 164, 108, 172, 173, 109, 175, 111,
        110, 174, 170, 106, 107, 171, 105, 169, 168, 104,
        120, 184, 185, 121, 187, 123, 122, 186, 190, 126,
        127, 191, 125, 189, 188, 124, 180, 116, 117, 181,
        119, 183, 182, 118, 114, 178, 179, 115, 177, 113,
        112, 176, 80, 144, 145, 81, 147, 83, 82, 146,
        150, 86, 87, 151, 85, 149, 148, 84, 156, 92,
        93, 157, 95, 159, 158, 94, 90, 154, 155, 91,
        153, 89, 88, 152, 136, 72, 73, 137, 75, 139,
        138, 74, 78, 142, 143, 79, 141, 77, 76, 140,
        68, 132, 133, 69, 135, 71, 70, 134, 130, 66,
        67, 131, 65, 129, 128, 64
        };

        public FujiErrorCode()
        {
        }

        public FujiErrorCode(string ComName)
        {
            IniFujiErrorCode();
            IniFujiSerialPort(ComName);
        }

        private void IniFujiSerialPort(string ComName)
        {
            try
            {
                _port = new SerialPort(ComName, 115200);
                _port.DataBits = 8;
                _port.Parity = Parity.None;
                _port.StopBits = StopBits.One;
                _port.WriteTimeout = 2000;
                _port.ReadTimeout = 2000;
                _port.ReceivedBytesThreshold = 1;
                LogManager.AddLog("串口打开成功");
                _port.DataReceived += SerialPortOnDataReceived;
            }
            catch (Exception ex)
            {
                LogManager.AddLog("串口读取错误:" + ex);
            }
        }

        public bool openCom()
        {
            try
            {
                if (!_port.IsOpen)
                {
                    _port.Open();
                }
                return true;
            }
            catch (Exception e)
            {
                LogManager.AddLog("串口打开失败," + e.Message);
                throw new Exception("串口打开失败," + e.Message);
            }
        }

        private void IniFujiErrorCode()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = basePath + "Devices\\setting.txt";
            if (!File.Exists(filePath))
            {
                return;
            }
            string re = File.ReadAllText(filePath);
            List<FujiStateCode> states = JsonConvert.DeserializeObject<List<FujiStateCode>>(re);
            foreach (FujiStateCode item in states)
            {
                fujiStateCodeDic.Add(item.Code, item);
            }
        }

        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = 0;
            byte[] _datas = new byte[5];
            List<byte> _tempdata = new List<byte>();
            while (_port.BytesToRead > 0)
            {
                byte c = (byte)_port.ReadByte();
                _tempdata.Add(c);
                count = _tempdata.Count;
                if (count == 5)
                {
                    _datas = _tempdata.ToArray();
                    _tempdata.Clear();
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();
                    break;
                }
            }
            uint crcdata = CRC16(_datas, 3u);
            if (_datas[3] != crcdata >> 8 || _datas[4] != (crcdata & 0xFF))
            {
                return;
            }
            LogManager.AddLog("校验正确:接受到数据");
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                int dataint = _datas[i];
                LogManager.AddLog($"第{i}个字符为:{dataint}");
                stringBuilder.Append(dataint);
            }
            if (Convert.ToInt32(stringBuilder.ToString()) == 102102102)
            {
                return;
            }
            if (fujiStateCodeDic.ContainsKey(stringBuilder.ToString()))
            {
                fujiStateCodeDic.TryGetValue(stringBuilder.ToString(), out fujiStateCodeNow);
            }
            else if (stringBuilder.ToString().StartsWith("102") || (stringBuilder.ToString().StartsWith("1") && stringBuilder.ToString().Length == 3))
            {
                fujiStateCodeNow = new FujiStateCode();
                LogManager.AddLog("stringBuilder" + stringBuilder.ToString());
                fujiStateCodeNow.Code = stringBuilder.ToString();
                fujiStateCodeNow.Msg = "就绪";
                fujiStateCodeNow.PState = 4;
            }
            else
            {
                fujiStateCodeNow = new FujiStateCode();
                fujiStateCodeNow.Code = stringBuilder.ToString();
                fujiStateCodeNow.Msg = "未知状态";
                if (Convert.ToInt32(fujiStateCodeNow.Code) > 200)
                {
                    fujiStateCodeNow.PState = 3;
                }
                else
                {
                    fujiStateCodeNow.PState = 4;
                }
            }
            if (fujiStateCodeNow != null)
            {
                LogManager.AddLog("当前状态：" + fujiStateCodeNow.Msg);
                LogManager.AddLog("当前状态码：" + fujiStateCodeNow.Code);
            }
        }

        private static uint CRC16(byte[] puchMsg, uint usDataLen)
        {
            int uIndex = 0;
            byte tempp = 0;
            uint CRCHL = 0u;
            byte uchCRCHi = byte.MaxValue;
            byte uchCRCLo = byte.MaxValue;
            while (usDataLen-- != 0)
            {
                uIndex = uchCRCHi ^ puchMsg[tempp];
                tempp = (byte)(tempp + 1);
                uchCRCHi = Convert.ToByte(uchCRCLo ^ auchCRCHi[uIndex]);
                uchCRCLo = auchCRCLo[uIndex];
            }
            CRCHL = uchCRCHi;
            CRCHL <<= 8;
            return CRCHL | uchCRCLo;
        }

        public bool SendData()
        {
            byte[] data = new byte[5] { 18, 52, 86, 71, 59 };
            if (_port.IsOpen)
            {
                try
                {
                    _port.Write(data, 0, data.Length);
                    Console.WriteLine("发送成功");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送失败" + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("串口未开启");
            }
            return false;
        }
    }
}
