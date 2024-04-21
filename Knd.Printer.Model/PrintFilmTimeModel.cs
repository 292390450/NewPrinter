using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudPrinter.Model
{
    public class PrintFilmTimeModel
    {
        public int PrintFilmCount { get; set; }

        public string PrintFilmSize { get; set; }

        public int PrintFilmTime { get; set; }
    }

    public class NewPrintFilmTimeModel
    {
        public int Index { get; set; }
        public ObservableCollection<PrintFilmTimeModel> PrintModels { get; set; }
    }
}
