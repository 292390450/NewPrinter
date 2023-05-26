using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Knd.Printer.CoreLibFrame45.ConfigHelper
{
    public class ConfigManager<T>
    {
        public static T Config;

        public static IBaseConfig<T> ConfigGenr;

        public static ConfigType ConfigType;

        public static string SavePath;

        public static void Init(string path, ConfigType configType, bool isLog)
        {
            SavePath = path;
            ConfigType = configType;
            switch (configType)
            {
                case ConfigType.Json:
                    ConfigGenr = new JsonConfig<T>();
                    break;
                case ConfigType.Xml:
                    ConfigGenr = new XmlConfig<T>();
                    break;
            }
            ConfigGenr.IsLog = isLog;
        }

        public static async Task<T> LoadAsync()
        {
            return Config = await ConfigGenr.LoadConfig(SavePath);
        }

        public static T Load()
        {
            return Config = ConfigGenr.LoadConfigSys(SavePath);
        }

        public static async Task Save()
        {
            await ConfigGenr.SaveConfig(SavePath, Config);
        }

        public static async Task<bool> GenraConfig()
        {
            return await ConfigGenr.GenrateConfig(SavePath);
        }
    }
    public enum ConfigType
    {
        Json,
        Xml
    }
    public interface IBaseConfig<T>
    {
        bool IsLog { get; set; }

        string ConfigStr { get; set; }

        Task<bool> GenrateConfig(string path);

        Task<T> LoadConfig(string path);

        T LoadConfigSys(string path);

        Task SaveConfig(string path, T config);
    }
    public class JsonConfig<T> : IBaseConfig<T>
    {
        private object Obj = new object();

        public bool IsLog { get; set; }

        public string ConfigStr { get; set; }

        public async Task<bool> GenrateConfig(string path)
        {
            return await Task.Run(delegate
            {
                try
                {
                    Type typeFromHandle = typeof(T);
                    object value = Activator.CreateInstance(typeFromHandle);
                    string text = JsonConvert.SerializeObject(value);
                    ConfigStr = text;
                    File.WriteAllText(path, text, new UTF8Encoding());
                    return true;
                }
                catch (Exception e)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e);
                    }
                    return false;
                }
            });
        }

        public async Task<T> LoadConfig(string path)
        {
            return await Task.Run(delegate
            {
                try
                {
                    if (File.Exists(path))
                    {
                        string text;
                        lock (Obj)
                        {
                            text = File.ReadAllText(path);
                        }
                        ConfigStr = text;
                        return JsonConvert.DeserializeObject<T>(text);
                    }
                    return default(T);
                }
                catch (Exception e)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e);
                    }
                    return default(T);
                }
            });
        }

        public T LoadConfigSys(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string configStr;
                    lock (Obj)
                    {
                        configStr = File.ReadAllText(path);
                    }
                    ConfigStr = configStr;
                    return JsonConvert.DeserializeObject<T>(configStr);
                }
                return default(T);
            }
            catch (Exception e)
            {
                if (IsLog)
                {
                    LogManager.AddLog(e);
                }
                return default(T);
            }
        }

        public async Task SaveConfig(string path, T config)
        {
            await Task.Run(delegate
            {
                try
                {
                    string text = JsonConvert.SerializeObject(config);
                    ConfigStr = text;
                    lock (Obj)
                    {
                        File.WriteAllText(path, text);
                    }
                }
                catch (Exception e)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e);
                    }
                }
            });
        }
    }

    public class XmlConfig<T> : IBaseConfig<T>
    {
        private object Obj = new object();

        public bool IsLog { get; set; }

        public string ConfigStr { get; set; }

        public async Task<bool> GenrateConfig(string path)
        {
            return await Task.Run(delegate
            {
                try
                {
                    Type typeFromHandle = typeof(T);
                    XElement xElement = CreateXelement(typeFromHandle, typeFromHandle.Name);
                    XDocument xDocument = new XDocument(new XComment("配置文件"), xElement);
                    ConfigStr = xDocument.ToString();
                    xDocument.Save(path);
                    return true;
                }
                catch (Exception e)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e);
                    }
                    return false;
                }
            });
        }

        public async Task<T> LoadConfig(string path)
        {
            return await Task.Run(delegate
            {
                try
                {
                    if (File.Exists(path))
                    {
                        XDocument xDocument;
                        lock (Obj)
                        {
                            xDocument = XDocument.Load(path);
                        }
                        ConfigStr = xDocument.ToString();
                        XElement root = xDocument.Root;
                        object obj = CreateEntity(root, typeof(T));
                        return (T)obj;
                    }
                    return default(T);
                }
                catch (Exception e)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e);
                    }
                    return default(T);
                }
            });
        }

        public async Task SaveConfig(string path, T config)
        {
            await Task.Run(delegate
            {
                try
                {
                    Type type = config.GetType();
                    XElement xElement = CreateXelement(type, type.Name);
                    AssignElement(xElement, config);
                    XDocument xDocument = new XDocument(new XComment(type.Name ?? ""), xElement);
                    xDocument.Save(path);
                }
                catch (Exception e)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e);
                    }
                }
            });
        }

        public XElement CreateXelement(Type type, string name)
        {
            PropertyInfo[] propers = type.GetProperties();
            XElement[] xElements = new XElement[propers.Length];
            for (int i = 0; i < propers.Length; i++)
            {
                XElement childElement = null;
                childElement = (xElements[i] = ((propers[i].PropertyType.IsPrimitive || !(propers[i].PropertyType != typeof(string)) || propers[i].PropertyType.IsValueType) ? new XElement(propers[i].Name, "") : CreateXelement(propers[i].PropertyType, propers[i].Name)));
            }
            XName name2 = name ?? "";
            object[] content = xElements;
            return new XElement(name2, content);
        }

        public object CreateEntity(XElement element, Type type)
        {
            object entity = Activator.CreateInstance(type);
            Type type2 = entity.GetType();
            IEnumerable<XElement> elemens = element.Elements();
            foreach (XElement xElement in elemens)
            {
                PropertyInfo propertyInfo = null;
                try
                {
                    propertyInfo = type2.GetProperty(xElement.Name.ToString());
                }
                catch (Exception e2)
                {
                    if (IsLog)
                    {
                        LogManager.AddLog(e2);
                    }
                }
                if (xElement.HasElements && propertyInfo != null)
                {
                    object child = CreateEntity(xElement, propertyInfo.PropertyType);
                    propertyInfo.SetValue(entity, child, null);
                }
                else
                {
                    if (string.IsNullOrEmpty(xElement.Value) || !(propertyInfo != null))
                    {
                        continue;
                    }
                    try
                    {
                        object v = Convert.ChangeType(xElement.Value, propertyInfo.PropertyType);
                        propertyInfo?.SetValue(entity, v, null);
                    }
                    catch (Exception e)
                    {
                        if (IsLog)
                        {
                            LogManager.AddLog(e);
                        }
                    }
                }
            }
            return entity;
        }

        public void AssignElement(XElement element, object eneity)
        {
            Type type = eneity.GetType();
            if (element.HasElements)
            {
                IEnumerable<XElement> elements = element.Elements();
                {
                    foreach (XElement xElement in elements)
                    {
                        object value = type.GetProperty(xElement.Name.ToString()).GetValue(eneity);
                        if (xElement.HasElements)
                        {
                            AssignElement(xElement, value);
                        }
                        else
                        {
                            xElement.Value = value.ToString();
                        }
                    }
                    return;
                }
            }
            element.Value = eneity.ToString();
        }

        public T LoadConfigSys(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    XDocument docu;
                    lock (Obj)
                    {
                        docu = XDocument.Load(path);
                    }
                    ConfigStr = docu.ToString();
                    XElement root = docu.Root;
                    object entity = CreateEntity(root, typeof(T));
                    return (T)entity;
                }
                return default(T);
            }
            catch (Exception e)
            {
                if (IsLog)
                {
                    LogManager.AddLog(e);
                }
                return default(T);
            }
        }
    }
}
