using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knd.Printer.CoreLibFrame45.StyletIoC
{
    public interface IContainer : IDisposable
    {
        void Compile(bool throwOnError = true);

        object Get(Type type, string key = null);

        T Get<T>(string key = null);

        IEnumerable<object> GetAll(Type type, string key = null);

        IEnumerable<T> GetAll<T>(string key = null);

        object GetTypeOrAll(Type type, string key = null);

        T GetTypeOrAll<T>(string key = null);

        void BuildUp(object item);
    }

}
