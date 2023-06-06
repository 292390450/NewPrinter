using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CloudPrinter.Model;
using Ost.AutoPrinter.Api;

namespace Knd.Printer.Model
{
    public class ClientInitResult
    {
        public int hosptail_id { get; set; }

        public int is_open_pay { get; set; }

        public string pay_key { get; set; }

        public string pay_url { get; set; }

        public string soft_url { get; set; }

        public string soft_versions { get; set; }
    }
    public enum FilmPrinterType
    {
        DicomPrinter,
        KndNewPrinter
    }
    public class InputPara
    {
        public string Num { get; set; }

        public string EqpCode { get; set; }

        public string ext { get; set; }
    }
    public class MqMessageModel
    {
        public int ewmCallbackType { get; set; }

        public string uploadClientId { get; set; }

        public string clientId { get; set; }

        public int ewmType { get; set; }

        public string timestamp { get; set; }

        public string accno { get; set; }
    }
    public enum PrinterState
    {
        就绪,
        打印中,
        错误,
        警告,
        未知
    }
    public enum PrinterType
    {
        Film,
        Report
    }
    public enum PrintOder
    {
        ReportFirst_先打报告,
        FilmFirst_先打胶片,
        MeanWhile_同时
    }
    public enum PrintType
    {
        OnlyReports,
        OnlyFilms,
        ReportsAndFilms,
        CloudOnly,
        ReportsAndFilmsAndCloud
    }
    public class ReceiveMessage
    {
        public enum MessageCategory
        {
            PrintDoc
        }

        public object HandlerParam { get; }

        public MessageCategory Category { get; private set; }

        public string ActionAccount { get; private set; }

        public DateTime StartTime { get; private set; }

        public bool IsProccess { get; set; }

        public ReceiveMessage(object handlerParam, string account)
        {
            HandlerParam = handlerParam;
            ActionAccount = account;
            IsProccess = false;
            StartTime = DateTime.Now;
            if (HandlerParam is PrintTaskInfo)
            {
                Category = MessageCategory.PrintDoc;
            }
        }
    }
    public enum SendFilmType
    {
        sendByServer,
        SendByClient
    }
    public class Setting
    {
        public bool IsOpenSelfService { get; set; }

        public string ClientId { get; set; }

        public string ServerId { get; set; }

        public int Port { get; set; }

        public int TimeOut { get; set; }

        public string HospitalName { get; set; }

        public PrintOder PrintOder { get; set; }

        public int ScreenCount { get; set; }

        public int ExtenScreenIndex { get; set; }

        public string BusinessMenInfo { get; set; }

        public int ScrollTime
        {
            get;
            set;
        }

        public bool IsDeleteTempFile { get; set; }

        public bool EnableLight { get; set; }

        public string LightCom { get; set; }

        public int LightRate { get; set; }

        public string PrinterIp { get; set; }

        public int PrinterPort { get; set; }

        public string PrinterAe { get; set; }

        public string ReportPrinterName { get; set; }

        public int ReportPrintTime { get; set; }

        public ObservableCollection<PrintFilmTimeModel> printFilmTimeModels { get; set; }

        public int PrintedReturnTime { get; set; }

        public SendFilmType sendFilmType { get; set; }

        public FilmPrinterType filmPrinterType { get; set; }

        public string clearErrorCode { get; set; }

        public string KeyboredStr { get; set; }

        public DicomType DicomType { get; set; }

        public bool PrinterIsColor { get; set; }

        public int Dpi { get; set; }

        public bool IsOpenColud { get; set; }

        public bool IsWaitState { get; set; }

        public string HubName { get; set; }

        public string SignalrUrl { get; set; }

        public string UploadClientId { get; set; }

        public double NameWidth { get; set; }

        public string NameReplace { get; set; }

        public double AccnoWidth { get; set; }

        public double PatientTypeWidth { get; set; }

        public double DtypeWidth { get; set; }

        public double ReportCountWidth { get; set; }

        public double FilmCountWidth { get; set; }

        public double ParamWidth { get; set; }

        public int InfoFontSize { get; set; }

        public int RefreshTime { get; set; }

        public int ShowInfoDay { get; set; }

        public Color InfoFontColor { get; set; }

        public double HospitalWidth { get; set; }
    }
}
