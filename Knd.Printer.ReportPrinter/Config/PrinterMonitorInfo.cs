using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knd.Printer.ReportPrinter.Config
{
    public class PrinterMonitorInfo
    {
        public int JobCount { get; set; }

        public uint StateCode { get; set; }
    }
    public class ReportPrinterConfig
    {
        public string Id { get; set; } = "ReportConfig";


        public string Name { get; set; }

        public int RefreshTime { get; set; }
    }
    public class ReportPrinterState
    {
        public uint Code;

        public int PState;

        public string Msg;
    }
}
