using Knd.Printer.CoreLibFrame45;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Knd.Printer.Core;

namespace CloudPrinter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Process RunningInstence()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcesses();
            Process[] array = processes;
            foreach (Process processe in array)
            {
                if (current.Id != processe.Id && current.ProcessName == processe.ProcessName)
                {
                    return processe;
                }
            }
            return null;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            try
            {
                var exist = RunningInstence();
                if (exist != null)
                {
                    if (exist.MainWindowHandle!=IntPtr.Zero)
                    {
                        MessageBox.Show("不能同时启动多个终端");
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        exist.Kill();
                    }
                   
                }
            }
            catch (Exception ex2)
            {
                LogManager.AddLog("程序启动异常：" + ex2.Message);
            }
            try
            {
                RunTimeHost.Initaial(this, new Assembly[1] { Assembly.GetExecutingAssembly() }, new string[3]
                {
                    AppDomain.CurrentDomain.BaseDirectory + "Devices",
                    AppDomain.CurrentDomain.BaseDirectory + "Api",
                    AppDomain.CurrentDomain.BaseDirectory
                });
                RunTimeHost.MEFContainer.GetExportedValue<MainWindow>().Show();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                LogManager.AddLog("程序初始化程序集异常：" + ex.Message);
            }
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogManager.AddLog("task线程未捕获异常：" + e.Exception.Message);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.AddLog("其他线程未捕获异常：" + e.ExceptionObject.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.AddLog("程序异常：" + e.Exception.Message);
            e.Handled = true;
        }
    }
}
