using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Knd.Printer.CoreLibFrame45;

namespace Knd.Printer.Core
{
    public class RunTimeHost
    {
        public static Dispatcher MainDispatcher { get; private set; }

        public static CompositionContainer MEFContainer { get; private set; }

        public static SimpleIoC SimpleIoC { get; set; }

        public static void Initaial(object app, Assembly[] startAssenblys = null, string[] modulePaths = null)
        {
            SimpleIoC = new SimpleIoC();
            MainDispatcher = ((Application)app).Dispatcher;
            AggregateCatalog catalog = new AggregateCatalog();
            if (startAssenblys != null)
            {
                foreach (Assembly startAssenbly in startAssenblys)
                {
                    catalog.Catalogs.Add(new AssemblyCatalog(startAssenbly));
                }
            }
            if (modulePaths != null)
            {
                foreach (string s in modulePaths)
                {
                    try
                    {
                        if (s == AppDomain.CurrentDomain.BaseDirectory)
                        {
                            DirectoryCatalog cat = new DirectoryCatalog(s, "*Api.dll");
                            catalog.Catalogs.Add(cat);
                        }
                        else
                        {
                            DirectoryCatalog cate = new DirectoryCatalog(s);
                            catalog.Catalogs.Add(cate);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.AddLog("加载程序集异常" + e.Message);
                    }
                }
            }
            CompositionContainer container = (MEFContainer = new CompositionContainer(catalog));
            MEFContainer.ComposeParts(app);
        }
    }

}
