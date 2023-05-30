using Knd.Printer.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Knd.Printer.Model;
using Ost.AutoPrinter.Api;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.FujiNetworkPrinter.Helpers;
using System.Reflection;
using System.Security.Cryptography;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.FujiNetworkPrinter.Models;
using Knd.Printer.FujiNetworkPrinter.Views;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using Knd.Printer.Core;
using System.Drawing;
using System.Collections.ObjectModel;
using Knd.Printer.FujiNetworkPrinter.ViewModels;
using Knd.Printer.Common.Helper;

namespace Knd.Printer.FujiNetworkPrinter
{
    [Export(typeof(IBaseDevice))]
    public class FujiNetworkPrinter : PrinterDevice, INotifyPropertyChanged
    {
        private Assembly _currentAssembly;
        private string basePath;
        private RSACryptoServiceProvider _gskRsa;
        private RSACryptoServiceProvider _pkRsa;
        private Socket _clientSocket;
        private bool _isStop;

        private string _status;

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        private string _job;

        public string Job
        {
            get
            {
                return _job;
            }
            set
            {
                _job = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<FilmBoxModel> _filmBoxs;

        public ObservableCollection<FilmBoxModel> FilmBoxs
        {
            get
            {
                return _filmBoxs;
            }
            set
            {
                _filmBoxs = value;
                OnPropertyChanged();
            }
        }
        public override PrinterState State { get; set; }
        public override PrinterType Type { get; set; } = PrinterType.Film;
        public override string Name { get; set; } = "3500网络相机";
        public override Action Selected { get; set; }
        public override void Check()
        {
            LogManager.AddLog(Name + "启动检查");
        }

        public override void Start()
        {
            try
            {
                //网络连接到相机
                string priKeyPath = basePath + "Keys\\" + "SignPriKey.pem";
                string pubKeyPath = basePath + "Keys\\" + "public.pem";
                RSAParameters gskpara = KeyHelper.LoadPkcsRsaPrivateKey(priKeyPath);
                _gskRsa = new RSACryptoServiceProvider();
                _gskRsa.ImportParameters(gskpara);
                RSAParameters pkpara = KeyHelper.LoadPkcsRsaPublicKey(pubKeyPath);
                _pkRsa = new RSACryptoServiceProvider();
                _pkRsa.ImportParameters(pkpara);
                if (IsOpenStateView)
                {
                    FilmBoxs = new ObservableCollection<FilmBoxModel>();
                        //创建连接
                    string ipStr = ConfigManager<FujiNetworkConfig>.Config.IpAddress;
                    IPAddress ip = IPAddress.Parse(ipStr);
                    var port = ConfigManager<FujiNetworkConfig>.Config.Port;
                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.Connect(new IPEndPoint(ip, port)); //配置服务器IP与端口 
                    _clientSocket = clientSocket;
                    ReadStatusThread();
                }
            }
            catch (Exception e)
            {
                LogManager.AddLog(e);
            }
        }

        private void ReadStatusThread()
        {
            Thread thread = new Thread((() =>
            {
                while (!_isStop)
                {
                    try
                    {
                       
                        if (_clientSocket==null)
                        {
                            break;
                        }
                        //状态
                        string cmd = "status";
                        string sign =KeyHelper.Sign(_gskRsa, cmd);
                        MyData d = new MyData();
                        d.msg = cmd;
                        d.sgn = sign;
                        string json = JsonConvert.SerializeObject(d);
                        string en =KeyHelper.SegmentEncryption(_pkRsa, json);
                        byte[] result = new byte[1024];
                        cmd = en;

                        string sendMessage = $"{cmd}{Environment.NewLine}";
                        _clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));
                        int receiveLength = _clientSocket.Receive(result);
                        var    status = $"{Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result, 0, receiveLength)))}";
                        RunTimeHost.MainDispatcher.Invoke(() =>
                        {
                            Status = status;
                        });
                        //任务数
                        Thread.Sleep(500);
                        cmd = "job";
                        sign = KeyHelper.Sign(_gskRsa, cmd);
                        d = new MyData();
                        d.msg = cmd;
                        d.sgn = sign;

                        json = JsonConvert.SerializeObject(d);
                        en = KeyHelper.SegmentEncryption(_pkRsa, json);

                        result = new byte[1024];
                        cmd = en;

                        sendMessage = $"{cmd}{Environment.NewLine}";
                        _clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));
                        receiveLength = _clientSocket.Receive(result);
                        var job = $"{Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result, 0, receiveLength)))}";
                        RunTimeHost.MainDispatcher.Invoke(() =>
                        {
                            Job = job;
                        });
                        //上片盒
                        Thread.Sleep(500);
                        cmd = "box 1";
                        sign = KeyHelper. Sign(_gskRsa, cmd);
                        d = new MyData();
                        d.msg = cmd;
                        d.sgn = sign;

                        json = JsonConvert.SerializeObject(d);
                        en = KeyHelper.SegmentEncryption(_pkRsa, json);

                        result = new byte[1024];
                        cmd = en;

                        sendMessage = $"{cmd}{Environment.NewLine}";
                        _clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));
                        receiveLength = _clientSocket.Receive(result);
                        var  box1 = $"{Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result, 0, receiveLength)))}";
                        var box1Model= ConvertBoxModel(box1);
                        if (box1Model != null)
                        {
                            box1Model.Location = "上片盒";

                        }
                        //下片盒
                        Thread.Sleep(500);
                        cmd = "box 2";
                        sign = KeyHelper.Sign(_gskRsa, cmd);
                        d = new MyData();
                        d.msg = cmd;
                        d.sgn = sign;

                        json = JsonConvert.SerializeObject(d);
                        en = KeyHelper.SegmentEncryption(_pkRsa, json);

                        result = new byte[1024];
                        cmd = en;

                        sendMessage = $"{cmd}{Environment.NewLine}";
                        _clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));
                        receiveLength = _clientSocket.Receive(result);
                        var box2 = $"{Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result, 0, receiveLength)))}";
                        var box2Model = ConvertBoxModel(box2);
                        if (box2Model != null)
                        {
                            box2Model.Location = "下片盒";
                        }
                        RunTimeHost.MainDispatcher.Invoke(() =>
                        {
                            if (box1Model != null)
                            {

                                var thisSize = FilmBoxs.FirstOrDefault(x => x.Size == box1Model.Size);
                                if (thisSize != null)
                                {
                                    thisSize.Count = box1Model.Count;
                                    thisSize.Location = box1Model.Location;
                                }
                                else
                                {
                                    FilmBoxs.Add(box1Model);
                                }
                              
                            }
                            if (box2Model != null)
                            {
                                var thisSize = FilmBoxs.FirstOrDefault(x => x.Size == box2Model.Size);
                                if (thisSize != null)
                                {
                                    thisSize.Count = box2Model.Count;
                                    thisSize.Location = box2Model.Location;
                                }
                                else
                                {
                                    FilmBoxs.Add(box2Model);
                                }
                            }
                           
                        });
                        //如果富士8000三个片盒
                        if (IsFuji8000)
                        {
                            Thread.Sleep(500);
                            cmd = "box 3";
                            sign = KeyHelper.Sign(_gskRsa, cmd);
                            d = new MyData();
                            d.msg = cmd;
                            d.sgn = sign;

                            json = JsonConvert.SerializeObject(d);
                            en = KeyHelper.SegmentEncryption(_pkRsa, json);

                            result = new byte[1024];
                            cmd = en;

                            sendMessage = $"{cmd}{Environment.NewLine}";
                            _clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));
                            receiveLength = _clientSocket.Receive(result);
                            var box3 = $"{Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result, 0, receiveLength)))}";
                            var box3Model = ConvertBoxModel(box3);
                            if (box3Model != null)
                            {
                                box3Model.Location = "下片盒2";
                                RunTimeHost.MainDispatcher.Invoke(() =>
                                {
                                    var thisSize = FilmBoxs.FirstOrDefault(x => x.Size == box3Model.Size);
                                    if (thisSize!=null)
                                    {
                                        thisSize.Count = box3Model.Count;
                                        thisSize.Location = box3Model.Location;
                                    }
                                    else
                                    {
                                        FilmBoxs.Add(box3Model);
                                    }
                                  
                                });
                            }
                        }
                       
                    }
                    catch (Exception e)
                    {
                        LogManager.AddLog(e);
                    }
                }
            }));
            thread.Start();
        }

        private FilmBoxModel ConvertBoxModel(string sizeinfo)
        {

            try
            {
                string box = sizeinfo.Trim();
                string[] b = box.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (b.Length > 2 || b.Length <= 0)
                {
                    LogManager.AddLog("尺寸信息转换失败");
                    return null;
                }
                string size = "";
                switch (b[0])
                {
                    case "1":
                        size = "8INX10IN";
                        break;
                    case "2":
                        size = "10INX14IN";
                        break;
                    case "3":
                        size = "14INX14IN";
                        break;
                    case "4":
                        size = "14INX17IN";
                        break;
                    case "5":
                        size = "10INX12IN";
                        break;
                    case "6":
                        size = "11INX14IN";
                        break;
                }

                FilmBoxModel filmBoxModel = new FilmBoxModel();
                filmBoxModel.Size = size;
           
                int a;
                if (int.TryParse(b[1], out a))
                {
                    filmBoxModel.Count = a;
                }
                else
                {
                    filmBoxModel.Count = 0;

                }
                return filmBoxModel;
            }
            catch (Exception e)
            {
                LogManager.AddLog(e);
                return null;
            }
        }
        public override void Stop()
        {
            _isStop = true;
        }

        public override void ResetFilmCountAsync(string num)
        {
           
        }

        public override void EndDocumentPrint(AbstractDocumentInfo documentInfo, bool isSuccess)
        {
           
        }

        public override void BeginDocumentPrint(AbstractDocumentInfo documentInfo)
        {
          
        }

        public override void EndTaskPrint(IList<PrintTaskInfo> taskInfos , bool isSuccess)
        {
            State = PrinterState.就绪;
        }

        public override void BeginTaskPrint(IList<PrintTaskInfo> taskInfos )
        {
            State = PrinterState.打印中;
            if (!ConfigManager<FujiNetworkConfig>.Config.IsOpen)
            {
                return;
            }
            IList<FilmDocumentInfo> films = new List<FilmDocumentInfo>();
            foreach (PrintTaskInfo item in taskInfos)
            {
                films.AddRange(item.Films);
            }
            Dictionary<string, int> filmBoxsDictionary = new Dictionary<string, int>();
            foreach (FilmBoxModel box in FilmBoxs)
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

        public async override void Initial()
        {
            LogManager.AddLog(Name + "初始化");
            _currentAssembly = GetType().Assembly;
            basePath = _currentAssembly.Location.Replace(_currentAssembly.ManifestModule.Name, "");
            string configPath = basePath + "Config\\" + GetType().Name + ".json";
            try
            {
                ConfigManager<FujiNetworkConfig>.Init(configPath, ConfigType.Json, isLog: true);
                if (!File.Exists(configPath))
                {
                    FujiNetworkConfig fujiPrinterConfigModel = new FujiNetworkConfig();
                  
                    fujiPrinterConfigModel.IsOpen = false;
                    fujiPrinterConfigModel.RefreshTime = 10;
                    ConfigManager<FujiNetworkConfig>.Config = fujiPrinterConfigModel;
                    RefreshTime = 100;
                    IsOpenStateView = false;
                    await ConfigManager<FujiNetworkConfig>.Save();
                }
                else
                {
                    ConfigManager<FujiNetworkConfig>.Load();
                    IsOpenStateView = ConfigManager<FujiNetworkConfig>.Config.IsOpen;
                    RefreshTime = ConfigManager<FujiNetworkConfig>.Config.RefreshTime;
                    IsFuji8000 = ConfigManager<FujiNetworkConfig>.Config.IsFuji8000;

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
            catch (Exception e)
            {
                LogManager.AddLog(e);
            }

        }

        [Import(typeof(SettingView))]
        public override ContentControl SettingView { get; set; }
        [Import(typeof(StateView))]
        public override ContentControl StateView { get; set; }
        private int _refreshTime;

        public int RefreshTime
        {
            get
            {
                return _refreshTime;
            }
            set
            {
                _refreshTime = value;
                OnPropertyChanged();
            }
        }

        private bool _isOpenStateView;
        public bool IsOpenStateView
        {
            get
            {
                return _isOpenStateView;
            }
            set
            {
                _isOpenStateView = value;
                OnPropertyChanged();
            }
        }
        private bool _isFuji8000;
        public bool IsFuji8000
        {
            get
            {
                return _isFuji8000;
            }
            set
            {
                _isFuji8000 = value;
                OnPropertyChanged();
            }
        }


        #region Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
