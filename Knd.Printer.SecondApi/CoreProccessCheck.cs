using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Knd.Printer.Common.Helper;
using Knd.Printer.CoreLibFrame45;
using Ost.AutoPrinter.Api;
using Ost.AutoPrinter.Api.Connection;
using Ost.AutoPrinter.Api.Impl;
using Ost.AutoPrinter.Api.Its;

namespace Knd.Printer.SecondApi
{
    public class CoreProccessCheck
    {
        public static void Check(ProccessAccNoInfo pInfo)
        {
            CheckPrintSettings(pInfo);
            CheckAccNoInfo(pInfo);
            CheckSettingInfo(pInfo);
            CheckPrintTime(pInfo);
            CheckPrintStep(pInfo);
        }

        private static void CheckPrintSettings(ProccessAccNoInfo pinfo)
        {
            IList<PrintTaskInfo> info = pinfo.AccNoInfo;
            ClientPrinterSetting setting = pinfo.Setting;
            switch (setting.PrintStep)
            {
                case PrintStep.FilmOnly:
                    {
                        foreach (PrintTaskInfo printTaskInfo2 in info)
                        {
                            printTaskInfo2.Reports.Clear();
                        }
                        break;
                    }
                case PrintStep.ReportOnly:
                    {
                        foreach (PrintTaskInfo printTaskInfo in info)
                        {
                            printTaskInfo.Films.Clear();
                        }
                        break;
                    }
            }
        }

        public static void CheckAccNoInfo(ProccessAccNoInfo pinfo)
        {
            if (pinfo.TaskInfos == null || pinfo.TaskInfos.Count == 0)
            {
                AudioHelper.Speak("该检查号无可打印内容");
                throw new Exception("该检查号无可打印内容");
            }
            if (pinfo.TaskInfos.Count > 1)
            {
                List<string> canPrints = new List<string>();
                List<string> noCanPrints = new List<string>();
                foreach (ProccessTaskInfo task in pinfo.TaskInfos)
                {
                    if (!task.PrintTask.CanPrint)
                    {
                        noCanPrints.Add(task.Id);
                        continue;
                    }
                    FilmDocumentInfo[] fs2 = task.PrintTask.GetPrintFilms();
                    ReportDocumentInfo[] rs2 = task.PrintTask.GetPrintReports();
                    if (fs2.Length == 0 && rs2.Length == 0)
                    {
                        noCanPrints.Add(task.Id);
                    }
                    else
                    {
                        canPrints.Add(task.Id);
                    }
                }
                if (canPrints.Count > 0)
                {
                    foreach (string i in noCanPrints)
                    {
                        pinfo.TaskInfos.Remove(pinfo.TaskInfos.SingleOrDefault((ProccessTaskInfo o) => o.Id == i));
                        pinfo.AccNoInfo.Remove(pinfo.AccNoInfo.SingleOrDefault((PrintTaskInfo o) => o.Id == i));
                    }
                    return;
                }
                ProccessTaskInfo p2 = pinfo.TaskInfos.SingleOrDefault((ProccessTaskInfo o) => o.Id == noCanPrints[0]);
                FilmDocumentInfo[] fs3 = p2.PrintTask.GetPrintFilms();
                ReportDocumentInfo[] rs3 = p2.PrintTask.GetPrintReports();
                if (!p2.PrintTask.CanPrint)
                {
                    if (p2.PrintTask.State == DocumentState.Printed)
                    {
                        AudioHelper.Speak("任务已在" + p2.PrintTask.PrintTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "打印");
                        throw new Exception("任务已在" + p2.PrintTask.PrintTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "打印");
                    }
                    if (fs3.Length == 0 && rs3.Length == 0)
                    {
                        AudioHelper.Speak("任务没有打印内容");
                        throw new Exception("任务没有打印内容");
                    }
                    if (fs3.Length == 0)
                    {
                        AudioHelper.Speak("胶片不存在, 请耐心等待.");
                        throw new Exception("胶片不存在, 请耐心等待.");
                    }
                    if (rs3.Length == 0)
                    {
                        AudioHelper.Speak("报告不存在, 请耐心等待.");
                        throw new Exception("报告不存在, 请耐心等待.");
                    }
                }
                if (fs3.Length == 0 && rs3.Length == 0)
                {
                    AudioHelper.Speak("任务没有打印内容");
                    throw new Exception("任务没有打印内容");
                }
                return;
            }
            ProccessTaskInfo p = pinfo.TaskInfos[0];
            FilmDocumentInfo[] fs = p.PrintTask.GetPrintFilms();
            ReportDocumentInfo[] rs = p.PrintTask.GetPrintReports();
            if (!p.PrintTask.CanPrint)
            {
                if (p.PrintTask.State == DocumentState.Printed)
                {
                    AudioHelper.Speak("任务已在" + p.PrintTask.PrintTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "打印");
                    throw new Exception("任务已在" + p.PrintTask.PrintTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "打印");
                }
                if (fs.Length == 0 && rs.Length == 0)
                {
                    AudioHelper.Speak("任务没有打印内容");
                    throw new Exception("任务没有打印内容");
                }
                if (fs.Length == 0)
                {
                    AudioHelper.Speak("胶片不存在, 请耐心等待.");
                    throw new Exception("胶片不存在, 请耐心等待.");
                }
                if (rs.Length == 0)
                {
                    AudioHelper.Speak("报告不存在, 请耐心等待.");
                    throw new Exception("报告不存在, 请耐心等待.");
                }
            }
            if (fs.Length != 0 || rs.Length != 0)
            {
                return;
            }
            AudioHelper.Speak("任务没有打印内容");
            throw new Exception("任务没有打印内容");
        }

        public static void CheckPrintStep(ProccessAccNoInfo pInfo)
        {
            ClientPrinterSetting setting = pInfo.Setting;
            IList<PrintTaskInfo> ainfo = pInfo.AccNoInfo;
            foreach (PrintTaskInfo accinfo in pInfo.AccNoInfo)
            {
                if (accinfo.State == DocumentState.Printing)
                {
                    string msg = "打印任务正在其他设备上打印. 请不要重复获取胶片.";
                    AudioHelper.Speak(msg);
                    throw new Exception(msg);
                }
            }
        }

        public static void CheckSettingInfo(ProccessAccNoInfo pinfo)
        {
            string[] rooms = pinfo.Setting.DTypes;
            if (rooms != null && rooms.Length != 0)
            {
                List<string> noType = new List<string>();
                foreach (ProccessTaskInfo info2 in pinfo.TaskInfos)
                {
                    string dtype = info2.PrintTask.DType;
                    if (!string.IsNullOrEmpty(dtype) && !rooms.Contains(dtype, StringComparer.OrdinalIgnoreCase))
                    {
                        noType.Add(info2.Id);
                    }
                }
                foreach (string id2 in noType)
                {
                    pinfo.TaskInfos.Remove(pinfo.TaskInfos.SingleOrDefault((ProccessTaskInfo o) => o.Id == id2));
                    pinfo.AccNoInfo.Remove(pinfo.AccNoInfo.SingleOrDefault((PrintTaskInfo o) => o.Id == id2));
                }
                if (pinfo.TaskInfos.Count == 0)
                {
                    AudioHelper.Speak("胶片类型不匹配, 请到其他打印设备上打印.");
                    throw new Exception("胶片类型不匹配, 请到其他打印设备上打印.");
                }
            }
            string[] sizes = pinfo.Setting.FilmSizes;
            if (sizes == null || sizes.Length == 0)
            {
                return;
            }
            List<string> noSize = new List<string>();
            foreach (ProccessTaskInfo info in pinfo.TaskInfos)
            {
                FilmDocumentInfo[] printFilms = info.PrintTask.GetPrintFilms();
                foreach (FilmDocumentInfo film in printFilms)
                {
                    string f = film.Size;
                    if (!sizes.Contains(f, StringComparer.OrdinalIgnoreCase))
                    {
                        noSize.Add(info.Id);
                    }
                }
            }
            foreach (string id in noSize)
            {
                pinfo.TaskInfos.Remove(pinfo.TaskInfos.SingleOrDefault((ProccessTaskInfo o) => o.Id == id));
                pinfo.AccNoInfo.Remove(pinfo.AccNoInfo.SingleOrDefault((PrintTaskInfo o) => o.Id == id));
            }
            if (pinfo.TaskInfos.Count == 0)
            {
                AudioHelper.Speak("胶片类型不匹配, 请到其他打印设备上打印.");
                throw new Exception("胶片类型不匹配, 请到其他打印设备上打印.");
            }
        }

        public void CheckCanPrintTime(ProccessAccNoInfo pInfo)
        {
            if (pInfo.AccNoInfo.Count > 1)
            {
                List<string> canPrints = new List<string>();
                List<string> noCanPrints = new List<string>();
                for (int j = 0; j < pInfo.AccNoInfo.Count; j++)
                {
                    PrintTaskInfo task = pInfo.AccNoInfo[j];
                    if (task.CanPrintTime.HasValue)
                    {
                        if (task.LeftCanPrintTime > 0)
                        {
                            noCanPrints.Add(task.Id);
                        }
                        else
                        {
                            canPrints.Add(task.Id);
                        }
                    }
                }
                if (canPrints.Count > 0)
                {
                    foreach (string i in noCanPrints)
                    {
                        pInfo.AccNoInfo.Remove(pInfo.AccNoInfo.SingleOrDefault((PrintTaskInfo o) => o.Id == i));
                        pInfo.TaskInfos.Remove(pInfo.TaskInfos.SingleOrDefault((ProccessTaskInfo o) => o.Id == i));
                    }
                    return;
                }
                PrintTaskInfo t = pInfo.AccNoInfo.SingleOrDefault((PrintTaskInfo o) => o.Id == noCanPrints[0]);
                if (!t.CanPrintTime.HasValue)
                {
                    return;
                }
                TimeSpan time = DateTime.Now - t.CanPrintTime.Value;
                if (t.LeftCanPrintTime > 0)
                {
                    int l = t.LeftCanPrintTime / 60;
                    if (t.LeftCanPrintTime % 60 > 0)
                    {
                        l++;
                    }
                }
                return;
            }
            foreach (PrintTaskInfo taskInfo in pInfo.AccNoInfo)
            {
                if (taskInfo.CanPrintTime.HasValue && taskInfo.LeftCanPrintTime > 0)
                {
                    int k = taskInfo.LeftCanPrintTime / 60;
                    if (taskInfo.LeftCanPrintTime % 60 > 0)
                    {
                        k++;
                    }
                }
            }
        }

        public static void CheckPrintTime(ProccessAccNoInfo pinfo)
        {
            if (pinfo.AccNoInfo[0].LeftCanPrintTime > 0)
            {
                int canPrintTime = pinfo.AccNoInfo[0].LeftCanPrintTime / 60;
                if (pinfo.AccNoInfo[0].LeftCanPrintTime % 60 > 0)
                {
                    canPrintTime++;
                    LogManager.AddLog("可打印时间：" + canPrintTime);
                    throw new Exception($"准备中，请于{canPrintTime}分钟后来取。");
                }
            }
        }
    }
    public class ProccessAccNoInfo
    {
        public const string RECEIVEEQPCODE = "SERVER";

        public string EqpCode { get; set; }

        public string Code { get; set; }

        public string Ext { get; set; }

        public bool IsServerProccess => EqpCode == "SERVER";

        public IList<PrintTaskInfo> AccNoInfo { get; private set; }

        public List<ProccessTaskInfo> TaskInfos { get; }

        public ClientPrinterSetting Setting { get; private set; }

        public object Param { get; set; }

        public ProccessAccNoInfo(IList<PrintTaskInfo> accNoInfo, ClientPrinterSetting setting)
        {
            AccNoInfo = accNoInfo;
            Setting = setting;
            if (accNoInfo != null)
            {
                TaskInfos = accNoInfo.Select((PrintTaskInfo t) => new ProccessTaskInfo(t, this)).ToList();
            }
        }
    }
    public class ProccessTaskInfo
    {
        public PrintTaskInfo PrintTask { get; }

        public ProccessAccNoInfo Proccess { get; }

        public BeginTaskResult BeginResult { get; set; }

        public string Id => PrintTask.Id;

        public string Token
        {
            get
            {
                return PrintTask.Token;
            }
            set
            {
                PrintTask.Token = value;
            }
        }

        public ProccessTaskInfo(PrintTaskInfo printTask, ProccessAccNoInfo proccess)
        {
            PrintTask = printTask;
            Proccess = proccess;
        }
    }
    [Export]
    public class SecondServiceApi
    {
        private ApiManager _apiManager;

        private IClientPrinterApi PrinterApi;

        private ServerState ServerState;

        private ClientPrinter ClientPrinter;

        private ClientPrinterSetting ClientSetting;

        private string _serverip;

        private int _port;

        private string _clientId;

        private string _key;

        private int _socketType;

        private int _timeOut;

        private bool IsConnected;

        public Action<bool> ConnectionChanged { get; set; }

        public Action<PrintDocArgs> OnRecieveMsg { get; set; }

        public BeginDocumentResult BeginFilmDocument(string docId, string taskId)
        {
            return PrinterApi.BeginFilmDocument(docId, taskId);
        }

        public object BeginPrint(string taskId, string eqp, string code, byte[] img)
        {
            return PrinterApi.BeginPrint(taskId, eqp, code, img);
        }

        public BeginDocumentResult BeginReportDocument(string docId, string taskId)
        {
            return PrinterApi.BeginReportDocument(docId, taskId);
        }

        public object EndFilmDocument(int token, DocumentState state, string msg)
        {
            return PrinterApi.EndFilmDocument(token, state, msg);
        }

        public bool EndPrint(string taskToken, DocumentState state, string msg)
        {
            try
            {
                return PrinterApi.EndPrint(taskToken, state, msg);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object EndReportDocument(int token, DocumentState state, string msg)
        {
            return PrinterApi.EndReportDocument(token, state, msg);
        }

        public Stream GetFilmStream(string pageId, int dpi = 200)
        {
            return PrinterApi.GetFilmStream(pageId, dpi);
        }

        public IList<PrintNotifyInfo> GetInfoEntities(DateTime date)
        {
            if (IsConnected)
            {
                return PrinterApi.GetAlearList(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0));
            }
            return null;
        }

        public Task<IList<PrintTaskInfo>> GetPrintTaskInfoAsync(string num, string eqp, string msg)
        {
            return Task.Run(delegate
            {
                Thread.Sleep(500);
                LogManager.AddLog("开始向服务端获取任务" + num);
                IList<PrintTaskInfo> printTaskInfo = PrinterApi.GetPrintTaskInfo(num, eqp, null);
                LogManager.AddLog("向服务端获取任务成功" + num);
                ProccessAccNoInfo proccessAccNoInfo = new ProccessAccNoInfo(printTaskInfo, ClientSetting);
                LogManager.AddLog("检查此任务是否具备打印条件" + num);
                CoreProccessCheck.Check(proccessAccNoInfo);
                proccessAccNoInfo.EqpCode = eqp;
                proccessAccNoInfo.Code = num;
                return proccessAccNoInfo.AccNoInfo;
            });
        }

        public Stream GetReportStream(string id, int index)
        {
            return PrinterApi.GetReportStream(id, index);
        }

        public void InitRemoteSerivce(string serverip, int port, string clientId, string key, int socketType, int timeOut)
        {
            _serverip = serverip;
            _port = port;
            _clientId = clientId;
            _key = key;
            _socketType = socketType;
            _timeOut = timeOut;
            try
            {
                _apiManager = new ApiManager();
                _apiManager.ConnectChanged += _apiManager_ConnectChanged;
                _apiManager.SocketType = socketType;
                _apiManager.TimeOut = timeOut;
                LogManager.AddLog("初始化服务端通信");
                _apiManager.Init(ClientType.AutoPrinter, serverip, port, clientId, key, start: false);
                LogManager.AddLog("服务端通信初始化成功");
                _apiManager.Start();
                LogManager.AddLog("开始服务端通信成功");
                PrinterApi = _apiManager.ClientPrinterApi;
                ServerState = PrinterApi.GetServerState();
                PrinterApi.RegisterConnect();
                ClientPrinter = _apiManager.ManagerApi.GetClientPlateform(clientId);
                ClientSetting = PrinterApi.GetClientSetting();
                IsConnected = true;
                PrinterApi.DocPrintEvent += PrinterApi_DocPrintEvent;
            }
            catch (Exception ex)
            {
                LogManager.AddLog("初始化服务端通信异常，异常信息：" + ex.Message);
                LogManager.AddLog("初始化服务端通信异常，异常堆栈：" + ex.StackTrace);
                throw new Exception("初始化服务端通信异常，异常信息：" + ex.Message);
            }
        }

        public void PrintFilmByServer(string DocumentInfoId, string PrintId)
        {
            try
            {
                LogManager.AddLog("使用服务器发送打印任务");
                if (!PrinterApi.SentFilmByServer(DocumentInfoId, DocType.Film, PrintId))
                {
                    LogManager.AddLog("使用服务器发送打印任务失败");
                    throw new Exception("服务器发送胶片文档失败");
                }
            }
            catch (Exception e)
            {
                LogManager.AddLog("使用服务器发送打印任务失败" + e.Message);
                throw e;
            }
        }

        public bool SendClientInfoToServer(string id, PaperInfo[] paperInfos, PrintState printState, string stateMsg)
        {
            PrinterStateInfo printerStateInfo = new PrinterStateInfo();
            try
            {
                printerStateInfo.Id = id;
                printerStateInfo.Online = -1;
                printerStateInfo.Papers = paperInfos;
                printerStateInfo.PrintState = printState;
                printerStateInfo.StateMsg = stateMsg;
                bool isSuccess = PrinterApi.UpdatePrinterInfo(printerStateInfo);
                if (!isSuccess)
                {
                    LogManager.AddLog("向服务端同步终端状态失败");
                }
                return isSuccess;
            }
            catch (Exception ex)
            {
                LogManager.AddLog("向服务端同步终端状态失败,错误信息为：" + ex.Message);
            }
            return false;
        }

        public bool GetServerState()
        {
            return IsConnected;
        }

        private void PrinterApi_DocPrintEvent(object sender, PrintDocArgs e)
        {
            OnRecieveMsg?.Invoke(e);
        }

        private void _apiManager_ConnectChanged(object sender, ConnectEventArgs e)
        {
            ConnectionChanged?.Invoke(e.Connected);
            IsConnected = e.Connected;
            Task checkServerTask = new Task(delegate
            {
                while (!IsConnected)
                {
                    Thread.Sleep(2000);
                    if (!IsConnected)
                    {
                        try
                        {
                            InitRemoteSerivce(_serverip, _port, _clientId, _key, _socketType, _timeOut);
                        }
                        catch (Exception ex)
                        {
                            LogManager.AddLog("重新初始化服务连接异常" + ex.Message);
                        }
                    }
                }
            });
            if (!IsConnected && checkServerTask.Status != TaskStatus.Running)
            {
                checkServerTask.Start();
            }
        }
    }
}
