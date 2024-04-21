using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using Aspose.Pdf;
using Aspose.Pdf.Facades;
using CloudPrinter;
using CloudPrinter.Model;
using CloudPrinter.View;
using CloudPrinter.View.ErrorViews;
using CloudPrinter.ViewModel.ErrorViews;
using GalaSoft.MvvmLight.Messaging;
using Knd.Printer.Abstract;
using Knd.Printer.Common.Helper;
using Knd.Printer.Core;
using Knd.Printer.CoreLibFrame45;
using Knd.Printer.CoreLibFrame45.Command;
using Knd.Printer.CoreLibFrame45.ConfigHelper;
using Knd.Printer.Model;
using Knd.Printer.SecondApi;
using Ost.AutoPrinter.Api;
using Ost.DicomService;

namespace CloudPrinter.ViewModel
{
    [Export]
    public class PrintingViewModel : ViewModelBase
    {
        private List<PrinterDevice> printers;

        private string _accno;

        private string _name;

        private int _reportCount;

        private int _filmCount;

        private int _printedReportCount;

        private int _printedFilmCount;

        private double _allProgress;

        private double _progress;

        private Visibility _qrCodeVisibility = Visibility.Collapsed;

        private BitmapSource _qrImage;

        private InputPara _currentInputPara;

        private System.Timers.Timer _timer;

        private int _printTime;

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

        public double AllProgress
        {
            get
            {
                return _allProgress;
            }
            set
            {
                _allProgress = value;
                NotifyPropertyChanged("AllProgress");
            }
        }

        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        public Visibility QrCodeVisibility
        {
            get
            {
                return _qrCodeVisibility;
            }
            set
            {
                _qrCodeVisibility = value;
                NotifyPropertyChanged("QrCodeVisibility");
            }
        }

        public override void InitaDataAsync()
        {
            IEnumerable<IBaseDevice> devices = RunTimeHost.MEFContainer.GetExportedValues<IBaseDevice>();
            printers = new List<PrinterDevice>();
            foreach (IBaseDevice baseDevice in devices)
            {
                if (baseDevice.GetType().BaseType == typeof(PrinterDevice))
                {
                    printers.Add((PrinterDevice)baseDevice);
                }
            }
            base.InitaDataAsync();
        }

        public async void InitSetTasks(IList<PrintTaskInfo> taskInfos, PrintType printType, InputPara inputPara)
        {
            LogManager.AddLog("打印方法执行");
            _currentInputPara = inputPara;
            Accno = taskInfos.FirstOrDefault()?.AccNo;
            Name = taskInfos.FirstOrDefault()?.PatientName;
            ReportCount = 0;
            FilmCount = 0;
            PrintedFilmCount = 0;
            PrintedReportCount = 0;
            QrCodeVisibility = Visibility.Collapsed;
            _printTime = 0;
            int filmNo = 1;
            foreach (PrintTaskInfo printTaskInfo in taskInfos)
            {
                ReportCount += printTaskInfo.CanPrintReportCount;
                FilmCount += printTaskInfo.CanPintFilmCount;
                foreach (FilmDocumentInfo item in printTaskInfo.Films)
                {
                    if (item.CanPrint)
                    {
                        int nowPrintFilmPrintTime = GetPrintTime(item.Size, filmNo);
                        _printTime += nowPrintFilmPrintTime;
                        filmNo++;
                    }
                }
            }
            System.Timers.Timer audioTimer = new System.Timers.Timer(8000.0);
            audioTimer.Elapsed += audioSpeak;
            audioTimer.Start();
            double addTime = (double)(_printTime + ReportCount * ConfigManager<Setting>.Config.ReportPrintTime) / 100.0;
            Progress = 0.0;
            AllProgress = 100.0;
            if (printType == PrintType.ReportsAndFilmsAndCloud)
            {
                QrImage = QrCodeHelper.GetBitmapImage(await GenerateQrImage(Accno, Name, FilmCount, ReportCount, ConfigManager<Setting>.Config.UploadClientId, ConfigManager<Setting>.Config.ClientId, RunTimeHost.SimpleIoC.Resolve<ClientInitResult>().pay_key));
                QrCodeVisibility = Visibility.Visible;
            }
            LogManager.AddLog("创建临时打印文件");
            string uri = AppDomain.CurrentDomain.BaseDirectory + "Temp";
            try
            {
                if (ConfigManager<Setting>.Config.IsDeleteTempFile)
                {
                    if (Directory.Exists(uri))
                    {
                        DirectoryInfo di = new DirectoryInfo(uri);
                        di.Delete(recursive: true);
                    }
                    Directory.CreateDirectory(uri);
                }
            }
            catch
            {
            }
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(addTime * 1000.0);
                _timer.Elapsed += _timer_Elapsed;
            }
            else
            {
                _timer.Interval = addTime * 1000.0;
            }
            try
            {
                LogManager.AddLog("执行各设备开始打印前的操作");
                foreach (PrinterDevice printerDevice3 in printers)
                {
                    printerDevice3.BeginTaskPrint(taskInfos);
                }
                string beforePrintAudio = ((Name == null || string.IsNullOrEmpty(Name)) ? "您有" : (Name + ",您有"));
                if (FilmCount > 0)
                {
                    beforePrintAudio += $"{FilmCount}张胶片";
                }
                if (ReportCount > 0)
                {
                    beforePrintAudio += $"{ReportCount}张诊断报告";
                }
                AudioHelper.Speak(beforePrintAudio);
                Messenger.Default.Send("Printing", "LightControl");
                await PrintTasks(taskInfos);
                foreach (PrinterDevice printerDevice2 in printers)
                {
                    printerDevice2.EndTaskPrint(taskInfos, isSuccess: true);
                }
                Messenger.Default.Send("PrintingFinish", "LightControl");
                audioTimer.Stop();
                audioTimer.Dispose();
                string PrintedAudio = "";
                if (FilmCount > 0 && ReportCount < 1)
                {
                    PrintedAudio = "打印完成,请取走您的胶片";
                }
                else if (ReportCount > 0 && FilmCount < 1)
                {
                    PrintedAudio = "打印完成,请取走您的报告";
                }
                else if (ReportCount > 0 && FilmCount > 0)
                {
                    PrintedAudio = "打印完成,请取走您的胶片和报告";
                }
                AudioHelper.Speak(PrintedAudio);
            }
            catch (Exception e)
            {
                try
                {
                    if (audioTimer != null)
                    {
                        audioTimer.Stop();
                        audioTimer.Dispose();
                    }
                }
                catch (Exception)
                {
                }
                foreach (PrinterDevice printerDevice in printers)
                {
                    printerDevice.EndTaskPrint(taskInfos, isSuccess: false);
                }
                ErrorView errorView = RunTimeHost.MEFContainer.GetExportedValue<ErrorView>();
                RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().ErrorMsg = "打印失败：" + e.Message;
                RunTimeHost.MEFContainer.GetExportedValue<ErrorViewModel>().InitaDataAsync();
                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = errorView;
                return;
            }
            LogManager.AddLog("跳转打印成功界面");
            PrintedView view = RunTimeHost.MEFContainer.GetExportedValue<PrintedView>();
            if (RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl.GetType() == typeof(PrintingView))
            {
                RunTimeHost.MEFContainer.GetExportedValue<MainViewModel>().CurrentControl = view;
                RunTimeHost.MEFContainer.GetExportedValue<PrintedViewModel>().Init(Name, Accno, FilmCount, ReportCount, PrintedFilmCount, PrintedReportCount, printType, QrImage);
            }
            AllProgress = 100.0;
            Progress = 0.0;
            _timer.Stop();
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

        private Task PrintTasks(IList<PrintTaskInfo> taskInfos)
        {
            return Task.Run(delegate
            {
                List<FilmDocumentInfo> list = new List<FilmDocumentInfo>();
                List<ReportDocumentInfo> list2 = new List<ReportDocumentInfo>();
                Dictionary<string, BeginTaskResult> dictionary = new Dictionary<string, BeginTaskResult>();
                foreach (PrintTaskInfo current in taskInfos)
                {
                    Accno = current.AccNo;
                    Name = current.PatientName;
                    BeginTaskResult beginTaskResult = new BeginTaskResult();
                    beginTaskResult = (BeginTaskResult)RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().BeginPrint(current.Id, _currentInputPara.EqpCode, _currentInputPara.Num, null);
                    LogManager.AddLog("告诉服务端开始打印文档" + Accno);
                    if (beginTaskResult.IsBegin)
                    {
                        LogManager.AddLog("文档已在打印中");
                        throw new Exception("文档已在打印中");
                    }
                    if (!string.IsNullOrEmpty(beginTaskResult.FailMessage))
                    {
                        string text = "检查号" + current.AccNo + "标记打印失败：" + beginTaskResult.FailMessage;
                        LogManager.AddLog(text);
                        throw new Exception(text);
                    }
                    dictionary.Add(current.Id, beginTaskResult);
                    FilmDocumentInfo[] printFilms = current.GetPrintFilms();
                    foreach (FilmDocumentInfo filmDocumentInfo in printFilms)
                    {
                        dictionary.Add(filmDocumentInfo.Id, beginTaskResult);
                        list.Add(filmDocumentInfo);
                    }
                    ReportDocumentInfo[] printReports = current.GetPrintReports();
                    foreach (ReportDocumentInfo reportDocumentInfo in printReports)
                    {
                        dictionary.Add(reportDocumentInfo.Id, beginTaskResult);
                        list2.Add(reportDocumentInfo);
                    }
                }
                _timer.Start();
                switch (ConfigManager<Setting>.Config.PrintOder)
                {
                    case PrintOder.FilmFirst_先打胶片:
                        try
                        {
                            PrintFilms(list.ToArray(), dictionary).Wait();
                            PrintReports(list2.ToArray(), dictionary).Wait();
                        }
                        catch (Exception innerException3)
                        {
                            if (innerException3.InnerException != null)
                            {
                                innerException3 = innerException3.InnerException;
                            }
                            foreach (PrintTaskInfo current4 in taskInfos)
                            {
                                dictionary.TryGetValue(current4.Id, out var value3);
                                RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndPrint(value3.Token, DocumentState.Error, innerException3.Message);
                            }
                            LogManager.AddLog(innerException3);
                            throw innerException3;
                        }
                        break;
                    case PrintOder.ReportFirst_先打报告:
                        try
                        {
                            PrintReports(list2.ToArray(), dictionary).Wait();
                            PrintFilms(list.ToArray(), dictionary).Wait();
                        }
                        catch (Exception innerException2)
                        {
                            if (innerException2.InnerException != null)
                            {
                                innerException2 = innerException2.InnerException;
                            }
                            foreach (PrintTaskInfo current3 in taskInfos)
                            {
                                dictionary.TryGetValue(current3.Id, out var value2);
                                RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndPrint(value2.Token, DocumentState.Error, innerException2.Message);
                            }
                            LogManager.AddLog(innerException2);
                            throw innerException2;
                        }
                        break;
                    case PrintOder.MeanWhile_同时:
                        try
                        {
                            Task.WaitAll(PrintReports(list2.ToArray(), dictionary), PrintFilms(list.ToArray(), dictionary));
                        }
                        catch (Exception innerException)
                        {
                            if (innerException.InnerException != null)
                            {
                                innerException = innerException.InnerException;
                            }
                            foreach (PrintTaskInfo current2 in taskInfos)
                            {
                                dictionary.TryGetValue(current2.Id, out var value);
                                RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndPrint(value.Token, DocumentState.Error, innerException.Message);
                            }
                            LogManager.AddLog(innerException);
                            throw innerException;
                        }
                        break;
                }
                LogManager.AddLog("打印结束，告诉服务端打印结束");
                foreach (PrintTaskInfo current5 in taskInfos)
                {
                    dictionary.TryGetValue(current5.Id, out var value4);
                    RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndPrint(value4.Token, DocumentState.Printed, "");
                }
            });
        }

        private Task PrintReports(ReportDocumentInfo[] reports, Dictionary<string, BeginTaskResult> taskToKenDictionary)
        {
            return Task.Run(delegate
            {
                ReportDocumentInfo[] array = reports;
                foreach (ReportDocumentInfo reportDocumentInfo in array)
                {
                    DateTime now = DateTime.Now;
                    taskToKenDictionary.TryGetValue(reportDocumentInfo.Id, out var value);
                    LogManager.AddLog("开始打印报告" + reportDocumentInfo.Id);
                    foreach (PrinterDevice current in printers)
                    {
                        current.BeginDocumentPrint(reportDocumentInfo);
                    }
                    Stream reportStream = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().GetReportStream(reportDocumentInfo.Id, 0);
                    if (reportStream == null)
                    {
                        throw new Exception("没有获取到报告内容");
                    }
                    string text = AppDomain.CurrentDomain.BaseDirectory + "Temp/" + reportDocumentInfo.Id + ".rep";
                    LogManager.AddLog("报告文件存入本地:" + text);
                    using (FileStream fileStream = File.Create(text))
                    {
                        reportStream.CopyTo(fileStream);
                        fileStream.Flush();
                        reportStream.Close();
                    }
                    BeginDocumentResult beginDocumentResult = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().BeginReportDocument(reportDocumentInfo.Id, value.Token);
                    if (!beginDocumentResult.IsSuccess)
                    {
                        throw new Exception("服务器标记胶片文档打印失败");
                    }
                    if (reportDocumentInfo.FileType == StreamType.PDF)
                    {
                        try
                        {
                            using FileStream content = File.OpenRead(text);
                            SendPdfToPrinter(content);
                        }
                        catch (Exception ex)
                        {
                            if (beginDocumentResult == null)
                            {
                                throw new Exception("请先执行文档打印标记.");
                            }
                            RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndReportDocument(beginDocumentResult.DocToken, DocumentState.Error, ex.Message);
                            foreach (PrinterDevice current2 in printers)
                            {
                                current2.EndDocumentPrint(reportDocumentInfo, isSuccess: false);
                            }
                            throw;
                        }
                        foreach (PrinterDevice current3 in printers)
                        {
                            current3.EndDocumentPrint(reportDocumentInfo, isSuccess: true);
                        }
                        RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndReportDocument(beginDocumentResult.DocToken, DocumentState.Printed, null);
                        int num = ConfigManager<Setting>.Config.ReportPrintTime - (int)(DateTime.Now - now).TotalSeconds;
                        if (num > 0)
                        {
                            Thread.Sleep(num * 1000);
                        }
                        PrintedReportCount += 1;
                        LogManager.AddLog("报告打印完成");
                    }
                }
            });
        }

        private Task PrintFilms(FilmDocumentInfo[] docs, Dictionary<string, BeginTaskResult> taskToKenDictionary)
        {
            return Task.Run(delegate
            {
                DateTime now = DateTime.Now;
                LogManager.AddLog("开始打印胶片");
                FilmDocumentInfo[] array = docs;
                foreach (FilmDocumentInfo filmDocumentInfo in array)
                {
                    taskToKenDictionary.TryGetValue(filmDocumentInfo.Id, out var value);
                    foreach (PrinterDevice current in printers)
                    {
                        current.BeginDocumentPrint(filmDocumentInfo);
                    }
                    BeginDocumentResult beginDocumentResult = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().BeginFilmDocument(filmDocumentInfo.Id, value.Token);
                    if (!beginDocumentResult.IsSuccess)
                    {
                        throw new Exception("服务器标记胶片文档打印失败");
                    }
                    try
                    {
                        if (ConfigManager<Setting>.Config.sendFilmType == SendFilmType.SendByClient)
                        {
                            ClientSendFilm(filmDocumentInfo);
                        }
                        else if (ConfigManager<Setting>.Config.sendFilmType == SendFilmType.sendByServer)
                        {
                            RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().PrintFilmByServer(filmDocumentInfo.Id, ConfigManager<Setting>.Config.PrinterIp);
                            Thread.Sleep(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (beginDocumentResult == null)
                        {
                            throw new Exception("请先执行文档打印标记.");
                        }
                        RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndFilmDocument(beginDocumentResult.DocToken, DocumentState.Error, ex.Message);
                        foreach (PrinterDevice current2 in printers)
                        {
                            current2.EndDocumentPrint(filmDocumentInfo, isSuccess: false);
                        }
                        throw new Exception("胶片发送失败：" + ex.Message);
                    }
                    foreach (PrinterDevice current3 in printers)
                    {
                        current3.EndDocumentPrint(filmDocumentInfo, isSuccess: true);
                    }
                    RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().EndFilmDocument(beginDocumentResult.DocToken, DocumentState.Printed, null);
                    if (filmDocumentInfo == docs.LastOrDefault())
                    {
                        int num = _printTime - (int)(DateTime.Now - now).TotalSeconds;
                        if (num > 0)
                        {
                            Thread.Sleep(num * 1000);
                        }
                        LogManager.AddLog("胶片任务打印完成");
                    }
                    PrintedFilmCount += 1;
                }
            });
        }

        private void ClientSendFilm(FilmDocumentInfo filmDocumentInfo)
        {
            Stream stream = RunTimeHost.MEFContainer.GetExportedValue<SecondServiceApi>().GetFilmStream(filmDocumentInfo.Id, ConfigManager<Setting>.Config.Dpi);
            if (stream == null)
            {
                throw new Exception("没有获取到图像内容");
            }
            LogManager.AddLog("获取到胶片图像");
            System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            stream.Close();
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
            string dir = AppDomain.CurrentDomain.BaseDirectory + "Temp\\" + fileName;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = dir + "\\" + fileName + ".Png";
            LogManager.AddLog("胶片图像保存到本地:" + path);
            using (FileStream sm = File.Create(path))
            {
                image.Save(sm, ImageFormat.Png);
                stream.Flush();
                image.Dispose();
            }
            if (ConfigManager<Setting>.Config.filmPrinterType == FilmPrinterType.DicomPrinter)
            {
                using (Bitmap temp = new Bitmap(path))
                {
                    DicomPrintImage(temp, filmDocumentInfo.FromAe, filmDocumentInfo.Orientation, filmDocumentInfo.Size);
                    return;
                }
            }
            if (ConfigManager<Setting>.Config.filmPrinterType == FilmPrinterType.KndNewPrinter)
            {
                ImageCodecInfo tiffCodecInfo = ImageCodecInfo.GetImageEncoders().First((ImageCodecInfo ie) => ie.MimeType == "image/tiff");
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, 18L);
                using (System.Drawing.Image pngimg = System.Drawing.Image.FromFile(path))
                {
                    pngimg.Save(path.Replace(".Png", ".tif"), tiffCodecInfo, encoderParams);
                }
                File.Delete(path);
                string xmlPath = path.Replace(".Png", ".xml");
                path = path.Replace(".Png", ".tif");
                filmDocumentInfo.Param = xmlPath;
                CreateFilmXml(xmlPath, path, filmDocumentInfo.Orientation, filmDocumentInfo.Size.Trim(), imageWidth, imageHeight);
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Progress >= AllProgress)
            {
                _timer.Stop();
            }
            Progress += 1.0;
        }

        private void audioSpeak(object sender, ElapsedEventArgs e)
        {
            AudioHelper.Speak("正在打印中，请勿离开");
        }

        private void SendPdfToPrinter(Stream content)
        {
            LogManager.AddLog("发送报告到报告打印机");
            Document doc = new Document(content);
            PdfViewer view = new PdfViewer(doc);
            try
            {
                view.PrintPageDialog = false;
                PrinterSettings ps = new PrinterSettings();
                PageSettings pgs = new PageSettings();
                ps.PrinterName = ConfigManager<Setting>.Config.ReportPrinterName;
                pgs.Margins = new Margins(0, 0, 0, 0);
                view.AutoResize = true;
                view.PrintDocumentWithSettings(pgs, ps);
                LogManager.AddLog("报告发送成功");
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                view.Close();
                doc.Dispose();
            }
        }

        private void DicomPrintImage(Bitmap img, string fromAe, string orientation, string size)
        {
            try
            {
                DicomPrintScuManager.Print(ConfigManager<Setting>.Config.DicomType.ToString(), fromAe, ConfigManager<Setting>.Config.PrinterAe, ConfigManager<Setting>.Config.PrinterIp, ConfigManager<Setting>.Config.PrinterPort, img, size.Trim(), orientation, ConfigManager<Setting>.Config.PrinterIsColor);
                LogManager.AddLog("胶片图像发送到相机成功");
            }
            catch (Exception e)
            {
                LogManager.AddLog("发送相机失败：" + e.Message);
            }
            Thread.Sleep(5000);
        }

        private void CreateFilmXml(string xmlPath, string filmPath, string orientation, string size, int imageWidth, int imageHeight)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration xmlSM = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDocument.AppendChild(xmlSM);
            XmlElement xmlFilmSession = xmlDocument.CreateElement("FilmSession");
            xmlFilmSession.SetAttribute("xmls", "http://www.efilming.com");
            xmlFilmSession.SetAttribute("NumberOfCopies", "1");
            xmlFilmSession.SetAttribute("PrintPriority", "MED");
            xmlFilmSession.SetAttribute("MediumType", "BLUE FILM");
            xmlFilmSession.SetAttribute("FilmSessionUID", "1.20190910140414296.1");
            xmlDocument.AppendChild(xmlFilmSession);
            XmlElement xmlPresentationLUT = xmlDocument.CreateElement("PresentationLUT");
            xmlFilmSession.AppendChild(xmlPresentationLUT);
            XmlElement xmlFilmBox = xmlDocument.CreateElement("FilmBox");
            xmlFilmBox.SetAttribute("BasicFilmBoxSOPInstanceUID", "1.20190910140414296.2");
            xmlFilmBox.SetAttribute("ImageDisplayFormat", "STANDARD\\1,1");
            xmlFilmBox.SetAttribute("FilmSizeID", size);
            xmlFilmBox.SetAttribute("FilmOrientation", orientation);
            xmlFilmBox.SetAttribute("MagnificationType", "CUBIC");
            xmlFilmBox.SetAttribute("BorderDensity", "BLACK");
            xmlFilmBox.SetAttribute("EmptyImageDensity", "BLACK");
            xmlFilmBox.SetAttribute("MinDensity", "10.0");
            xmlFilmBox.SetAttribute("MaxDensity", "150.0");
            xmlFilmBox.SetAttribute("ReferencedPresentationLUTSOPInstanceUID", "");
            xmlFilmSession.AppendChild(xmlFilmBox);
            XmlElement xmlImageBox = xmlDocument.CreateElement("ImageBox");
            xmlImageBox.SetAttribute("BasicGrayscaleImageBoxSOPInstanceUID", "1.20190910140414296.3");
            xmlImageBox.SetAttribute("ImageBoxPosition", "1");
            xmlImageBox.SetAttribute("SamplesPerPixel", "1");
            xmlImageBox.SetAttribute("PhotometricInterpretation", "MONOCHROME2");
            xmlImageBox.SetAttribute("Rows", imageHeight.ToString());
            xmlImageBox.SetAttribute("Columns", imageWidth.ToString());
            xmlImageBox.SetAttribute("BitsAllocated", "8");
            xmlImageBox.SetAttribute("BitsStored", "8");
            xmlImageBox.SetAttribute("HighBit", "7");
            xmlImageBox.SetAttribute("PixelRepresentation", "0");
            xmlImageBox.SetAttribute("PlanarConfiguration", "0");
            xmlImageBox.SetAttribute("PixelData", filmPath);
            xmlImageBox.SetAttribute("Polarity", "NORMAL");
            xmlImageBox.SetAttribute("PixelAspectRatio", "1\\1");
            xmlImageBox.SetAttribute("ReferencedPresentationLUTSOPInstanceUID", "");
            xmlImageBox.SetAttribute("PlanarConfiguration", "0");
            xmlFilmBox.AppendChild(xmlImageBox);
            xmlDocument.Save(xmlPath);
        }

        public int GetPrintTime(string size, int position)
        {
            int printTime = 0;
            //foreach (PrintFilmTimeModel item2 in ConfigManager<Setting>.Config.printFilmTimeModels)
            //{
            //    if (item2.PrintFilmSize == size && item2.PrintFilmCount == position)
            //    {
            //        printTime = item2.PrintFilmTime;
            //    }
            //}
            //if (printTime == 0)
            //{
            //    foreach (PrintFilmTimeModel item in ConfigManager<Setting>.Config.printFilmTimeModels)
            //    {
            //        if (item.PrintFilmSize == size)
            //        {
            //            printTime = item.PrintFilmTime;
            //        }
            //    }
            //}
            //if (printTime == 0)
            //{
            //    printTime = ConfigManager<Setting>.Config.printFilmTimeModels.FirstOrDefault().PrintFilmTime;
            //}
            try
            {
                var setp = ConfigManager<Setting>.Config.NewPrintFilmTimeModels?.Find(x => x.Index == position);
                if (setp!=null)
                {
                  var matchSize=  setp.PrintModels?.FirstOrDefault(x => x.PrintFilmSize == size);
                  if (matchSize!=null)
                  {
                      printTime = matchSize.PrintFilmTime;
                  }
                }

                if (printTime == 0)
                {
                    //没找到配置
                    printTime = ConfigManager<Setting>.Config.NewPrintFilmTimeModels?.FirstOrDefault()?.PrintModels
                        ?.FirstOrDefault()?.PrintFilmTime ?? 0;
                }

                if (printTime == 0)
                {
                    printTime = 60;
                }
            }
            catch (Exception e)
            {
                LogManager.AddLog(e);
            }

            return printTime;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            ImageCodecInfo[] array = codecs;
            foreach (ImageCodecInfo codec in array)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }

}
