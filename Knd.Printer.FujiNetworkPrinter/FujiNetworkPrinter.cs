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

namespace Knd.Printer.FujiNetworkPrinter
{
    [Export(typeof(IBaseDevice))]
    public class FujiNetworkPrinter : PrinterDevice, INotifyPropertyChanged
    {

      

        public override PrinterState State { get; set; }
        public override PrinterType Type { get; set; } = PrinterType.Film;
        public override string Name { get; set; } = "3500网络相机";
        public override Action Selected { get; set; }
        public override void Check()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override void ResetFilmCountAsync(string num)
        {
            throw new NotImplementedException();
        }

        public override void EndDocumentPrint(AbstractDocumentInfo documentInfo, bool isSuccess)
        {
            throw new NotImplementedException();
        }

        public override void BeginDocumentPrint(AbstractDocumentInfo documentInfo)
        {
            throw new NotImplementedException();
        }

        public override void EndTaskPrint(IList<PrintTaskInfo> taskInfos , bool isSuccess)
        {
            throw new NotImplementedException();
        }

        public override void BeginTaskPrint(IList<PrintTaskInfo> taskInfos )
        {
            throw new NotImplementedException();
        }

        public override void Initial()
        {
            throw new NotImplementedException();
        }

        public override ContentControl SettingView { get; set; }
        public override ContentControl StateView { get; set; }



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
