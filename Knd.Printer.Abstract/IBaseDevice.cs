using Knd.Printer.Model;
using Ost.AutoPrinter.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Knd.Printer.Abstract
{
    public interface IBaseDevice
    {
        string Name { get; set; }

        Action Selected { get; set; }

        ContentControl SettingView { get; set; }

        void Initial();

        void Check();

        void Start();

        void Stop();

        void ResetFilmCountAsync(string num);
    }
    public interface ISignalrServer
    {
        Action DisconnectionAction { get; set; }

        Action Connected { get; set; }

        Action<MqMessageModel> OnReciveMsg { get; set; }

        Task Init(Setting setting);
    }
    public abstract class PrinterDevice : IBaseDevice
    {
        public abstract PrinterState State { get; set; }

        public abstract PrinterType Type { get; set; }

        public abstract string Name { get; set; }

        public abstract ContentControl SettingView { get; set; }

        public abstract ContentControl StateView { get; set; }

        public abstract Action Selected { get; set; }

        public abstract void Check();

        public abstract void Initial();

        public abstract void Start();

        public abstract void Stop();

        public abstract void BeginTaskPrint(IList<PrintTaskInfo> taskInfo);

        public abstract void EndTaskPrint(IList<PrintTaskInfo> taskInfo, bool isSuccess);

        public abstract void BeginDocumentPrint(AbstractDocumentInfo documentInfo);

        public abstract void EndDocumentPrint(AbstractDocumentInfo documentInfo, bool isSuccess);

        public abstract void ResetFilmCountAsync(string num);
    }
}
