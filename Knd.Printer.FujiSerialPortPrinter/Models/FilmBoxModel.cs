using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knd.Printer.FujiSerialPortPrinter.Models
{
    public class FilmBoxModel
    {
        public string Size { get; set; }

        public int Count { get; set; } = 100;


        public string Location { get; set; }

        public int MaxCount { get; set; }
    }
    public class FujiPrinterConfigModel
    {
        public bool IsOpen { get; set; }

        public string Id { get; set; } = "FilmPrinterConfigModel";


        public FilmBoxModel[] FilmBoxs { get; set; }

        public bool IsOpenSerialPort { get; set; }

        public string ComName { get; set; }

        public int RefreshTime { get; set; }
    }
    public class FujiStateCode
    {
        public string Code;

        public int PState;

        public string Msg;
    }

}
