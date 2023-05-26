using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Knd.Printer.CoreLibFrame45
{
    public static class Extend
    {
        public static void AddRange<T>(this IList<T> source, IList<T> lists)
        {
            foreach (T var in lists)
            {
                source.Add(var);
            }
        }

        public static void AddRange<T>(this IList<T> source, IEnumerable<T> lists)
        {
            foreach (T var in lists)
            {
                source.Add(var);
            }
        }
    }
    public class LogManager
    {
        private static readonly string StorePath = AppDomain.CurrentDomain.BaseDirectory + "log";

        private static readonly object Obj = new object();

        public static void AddLog(string str)
        {
            CheckPath(StorePath);
            string error = "——————————\r\n" + DateTime.Now.ToString(CultureInfo.InstalledUICulture) + " | 日志信息:  " + str + "\r\n";
            string dateTime = DateTime.Now.ToShortDateString();
            dateTime = dateTime.Replace("/", "");
            try
            {
                lock (Obj)
                {
                    using FileStream fs = new FileStream(StorePath + "/" + dateTime + ".txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(error);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void AddLog(Exception e)
        {
            CheckPath(StorePath);
            string error = "——————————\r\n" + DateTime.Now.ToString(CultureInfo.InstalledUICulture) + " | 错误信息：" + e.Message + "\r\n导致错误的对象名称:" + e.Source + "\r\n引发异常的方法:" + e.TargetSite?.ToString() + "\r\n帮助链接:" + e.HelpLink + "\r\n调用堆:" + e.StackTrace + "\r\n";
            string dateTime = DateTime.Now.ToShortDateString();
            dateTime = dateTime.Replace("/", "");
            try
            {
                lock (Obj)
                {
                    using FileStream fs = new FileStream(StorePath + "/" + dateTime + ".txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(error);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

    public class SimpleIoC
    {
        private class EnteredObject
        {
            private readonly bool _isSinglton;

            public Type LiveType { get; private set; }

            public object SingletonInstance { get; private set; }

            public EnteredObject(Type liveType, bool isSingleton, object instance)
            {
                _isSinglton = isSingleton;
                LiveType = liveType;
                SingletonInstance = instance;
            }

            public object CreateInstance(params object[] args)
            {
                object instance = Activator.CreateInstance(LiveType, args);
                if (_isSinglton)
                {
                    SingletonInstance = instance;
                }
                return instance;
            }
        }

        private readonly IDictionary<Type, EnteredObject> _registeredObjects = new Dictionary<Type, EnteredObject>();

        public SimpleIoC()
        {
            RegisterInstance(this);
        }

        public void Register<TType>() where TType : class
        {
            Register<TType, TType>(isSingleton: false, null);
        }

        public void Register<TType, TLive>() where TLive : class, TType
        {
            Register<TType, TLive>(isSingleton: false, null);
        }

        public void RegisterSingleton<TType>() where TType : class
        {
            RegisterSingleton<TType, TType>();
        }

        public void RegisterSingleton<TType, TLive>() where TLive : class, TType
        {
            Register<TType, TLive>(isSingleton: true, null);
        }

        public void RegisterInstance<TType>(TType instance) where TType : class
        {
            RegisterInstance<TType, TType>(instance);
        }

        public void RegisterInstance<TType, TLive>(TLive instance) where TLive : class, TType
        {
            Register<TType, TLive>(isSingleton: true, instance);
        }

        public TResolve Resolve<TResolve>()
        {
            return (TResolve)ResolveObject(typeof(TResolve));
        }

        public object Resolve(Type type)
        {
            return ResolveObject(type);
        }

        private void Register<TType, TLive>(bool isSingleton, TLive instance)
        {
            Type type = typeof(TType);
            if (_registeredObjects.ContainsKey(type))
            {
                _registeredObjects.Remove(type);
            }
            _registeredObjects.Add(type, new EnteredObject(typeof(TLive), isSingleton, instance));
        }

        private object ResolveObject(Type type)
        {
            EnteredObject registeredObject = _registeredObjects[type];
            if (registeredObject == null)
            {
                throw new ArgumentOutOfRangeException($"The type {type.Name} has not been registered");
            }
            return GetInstance(registeredObject);
        }

        private object GetInstance(EnteredObject registeredObject)
        {
            object instance = registeredObject.SingletonInstance;
            if (instance == null)
            {
                IEnumerable<object> parameters = ResolveConstructorParameters(registeredObject);
                instance = registeredObject.CreateInstance(parameters.ToArray());
            }
            return instance;
        }

        private IEnumerable<object> ResolveConstructorParameters(EnteredObject registeredObject)
        {
            ConstructorInfo constructorInfo = registeredObject.LiveType.GetConstructors().First();
            return from parameter in constructorInfo.GetParameters()
                   select ResolveObject(parameter.ParameterType);
        }
    }
    public class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isBusy;

        private bool _isValid = true;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                NotifyPropertyChanged("IsBusy");
            }
        }

        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
                NotifyPropertyChanged("IsValid");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase()
        {
            InitaDataAsync();
            InitData();
            InitCommand();
        }

        public virtual async void InitaDataAsync()
        {
        }

        public virtual void InitData()
        {
        }

        public virtual void InitCommand()
        {
        }

        public virtual void RestartApp()
        {
            System.Windows.Application.Current.Shutdown();
            System.Windows.Forms.Application.Restart();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
